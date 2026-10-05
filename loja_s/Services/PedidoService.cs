using loja_s.Data;
using loja_s.Models;
using loja_s.ViewModels;

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
        var subtotal = itens.Sum(i => i.Quantidade * i.PrecoUnitario);
        var frete = subtotal > 0 ? 19.90m : 0m;
        var total = subtotal + frete - model.Desconto;

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
            NumeroPedido = "FB" + DateTime.UtcNow.ToString("yyMMddHHmmss"),
            DataPedido = DateTime.UtcNow,
            Status = "Pendente",
            Subtotal = subtotal,
            Frete = frete,
            Desconto = model.Desconto,
            ValorTotal = total,
            FormaPagamento = model.FormaPagamento,
            StatusPagamento = "Pendente",
            EnderecoEntregaId = endereco.Id
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        foreach (var item in itens)
        {
            var produto = await _context.Produtos.FindAsync(item.ProdutoId);
            if (produto == null) continue;

            _context.ItensPedido.Add(new ItemPedido
            {
                PedidoId = pedido.Id,
                ProdutoId = produto.Id,
                NomeProduto = produto.Nome,
                Quantidade = item.Quantidade,
                PrecoUnitario = produto.Preco
            });
        }

        await _context.SaveChangesAsync();

        var resultadoEstoque = true;
        foreach (var item in itens)
        {
            resultadoEstoque = await _estoqueService.DebitarEstoqueAsync(item.ProdutoId, item.Quantidade, usuarioId, $"Venda do pedido {pedido.NumeroPedido}");
            if (!resultadoEstoque)
            {
                break;
            }
        }

        if (!resultadoEstoque)
        {
            pedido.Status = "Cancelado";
            pedido.StatusPagamento = "Cancelado";
            await _context.SaveChangesAsync();
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
        return pedido;
    }
}
