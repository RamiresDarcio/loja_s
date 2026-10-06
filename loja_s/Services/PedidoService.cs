using loja_s.Data;
using loja_s.Models;
using loja_s.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Services;

public class PedidoService
{
    private readonly ApplicationDbContext _context;
    private readonly EstoqueService _estoqueService;
    private readonly IPagamentoService _pagamentoService;

    public PedidoService(ApplicationDbContext context, EstoqueService estoqueService, IPagamentoService pagamentoService)
    {
        _context = context;
        _estoqueService = estoqueService;
        _pagamentoService = pagamentoService;
    }

    public async Task<Pedido> CriarPedidoAsync(CheckoutViewModel model, List<ItemCarrinho> itens, int usuarioId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        if (itens.Count == 0 || itens.Any(i => i.Quantidade <= 0))
        {
            throw new InvalidOperationException("O pedido precisa conter produtos com quantidades válidas.");
        }

        var quantidades = itens.GroupBy(i => i.ProdutoId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));
        var produtos = await _context.Produtos
            .Where(p => quantidades.Keys.Contains(p.Id) && p.Status == "Ativo")
            .ToDictionaryAsync(p => p.Id);
        if (produtos.Count != quantidades.Count)
        {
            throw new InvalidOperationException(
                "Um produto do carrinho não está mais disponível. Revise seu carrinho antes de finalizar a compra.");
        }

        var linhasPedido = quantidades.Select(par =>
        {
            var produto = produtos[par.Key];
            if (par.Value > produto.Estoque)
            {
                throw new InvalidOperationException(
                    $"Estoque insuficiente para {produto.Nome}. Atualize o carrinho antes de concluir.");
            }

            return (Produto: produto, Quantidade: par.Value, PrecoUnitario: produto.PrecoEfetivo);
        }).ToList();
        var subtotal = linhasPedido.Sum(i => i.Quantidade * i.PrecoUnitario);
        var frete = subtotal > 0 ? 19.90m : 0m;
        const decimal desconto = 0m;
        var total = subtotal + frete;

        var endereco = new Endereco
        {
            UsuarioId = usuarioId,
            CEP = model.CEP,
            Rua = model.Rua,
            Numero = model.Numero,
            Complemento = model.Complemento,
            Bairro = model.Bairro,
            Cidade = model.Cidade,
            Estado = model.Estado,
            Nome = "Entrega"
        };

        _context.Enderecos.Add(endereco);
        await _context.SaveChangesAsync();

        var pedido = new Pedido
        {
            UsuarioId = usuarioId,
            NumeroPedido = "FB" + DateTime.UtcNow.ToString("yyMMddHHmmss") + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
            DataPedido = DateTime.UtcNow,
            Status = "Pendente",
            Subtotal = subtotal,
            Frete = frete,
            Desconto = desconto,
            ValorTotal = total,
            FormaPagamento = model.FormaPagamento,
            StatusPagamento = "Pendente",
            EnderecoEntregaId = endereco.Id
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        foreach (var item in linhasPedido)
        {
            _context.ItensPedido.Add(new ItemPedido
            {
                PedidoId = pedido.Id,
                ProdutoId = item.Produto.Id,
                NomeProduto = item.Produto.Nome,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario
            });
        }

        await _context.SaveChangesAsync();

        var resultadoEstoque = true;
        foreach (var item in linhasPedido)
        {
            resultadoEstoque = await _estoqueService.DebitarEstoqueAsync(
                item.Produto.Id,
                item.Quantidade,
                usuarioId,
                $"Venda do pedido {pedido.NumeroPedido}");
            if (!resultadoEstoque)
            {
                break;
            }
        }

        if (!resultadoEstoque)
        {
            throw new InvalidOperationException("Não foi possível concluir a compra porque o estoque não está disponível para todos os itens.");
        }

        var pagamento = await _pagamentoService.ProcessarAsync(pedido, model.FormaPagamento, null);
        pedido.Pagamento = pagamento;
        pedido.StatusPagamento = pagamento.Status;
        if (pagamento.Status == "Pago")
        {
            pedido.Status = "Finalizado";
        }
        else if (pagamento.Status == "AguardandoPagamento")
        {
            pedido.Status = "AguardandoPagamento";
        }

        await _context.SaveChangesAsync();

        var receberAtualizacoes = await _context.PerfisConta
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => (bool?)p.AtualizacoesPedidos)
            .FirstOrDefaultAsync();
        if (receberAtualizacoes != false)
        {
            _context.Notificacoes.Add(new Notificacao
            {
                UsuarioId = usuarioId,
                Categoria = "AtualizacoesPedidos",
                Titulo = "Pedido registrado",
                Mensagem = $"O pedido {pedido.NumeroPedido} foi registrado com status {pedido.Status}.",
                Url = $"/Pedido/Detalhes/{pedido.Id}"
            });
            await _context.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return pedido;
    }
}
