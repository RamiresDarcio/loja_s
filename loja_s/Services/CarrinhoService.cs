using System.Text.Json;
using loja_s.Data;
using loja_s.Models;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Services;

public class CarrinhoService
{
    private const string CarrinhoSessionKey = "felibow.carrinho";
    private readonly ISession _session;
    private readonly ApplicationDbContext _context;

    public CarrinhoService(ISession session, ApplicationDbContext context)
    {
        _session = session;
        _context = context;
    }

    public async Task<List<ItemCarrinho>> ObterItensAsync(int? usuarioId)
    {
        if (usuarioId is > 0)
        {
            var carrinhoConta = await _context.Carrinhos.Include(c => c.Itens)
                .Where(c => c.UsuarioId == usuarioId.Value)
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();
            return carrinhoConta?.Itens.OrderBy(i => i.Id).ToList() ?? [];
        }

        return ObterCarrinhoDaSessao().Itens;
    }

    public async Task<bool> SincronizarComCatalogoAsync(int? usuarioId)
    {
        var carrinhoConta = usuarioId is > 0
            ? await ObterCarrinhoAsync(usuarioId.Value)
            : null;
        var carrinhoSessao = usuarioId is > 0 ? null : ObterCarrinhoDaSessao();
        var itens = carrinhoConta?.Itens ?? carrinhoSessao?.Itens;
        if (itens == null || itens.Count == 0)
        {
            return false;
        }

        var produtoIds = itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await _context.Produtos
            .Where(p => produtoIds.Contains(p.Id) && p.Status == "Ativo")
            .ToDictionaryAsync(p => p.Id);
        var alterado = false;

        foreach (var item in itens.ToList())
        {
            if (!produtos.TryGetValue(item.ProdutoId, out var produto) || produto.Estoque <= 0)
            {
                if (carrinhoConta != null)
                {
                    _context.ItensCarrinho.Remove(item);
                }
                else
                {
                    carrinhoSessao!.Itens.Remove(item);
                }

                alterado = true;
                continue;
            }

            if (item.Quantidade > produto.Estoque)
            {
                item.Quantidade = produto.Estoque;
                alterado = true;
            }

            if (item.NomeProduto != produto.Nome || item.PrecoUnitario != produto.PrecoEfetivo)
            {
                item.NomeProduto = produto.Nome;
                item.PrecoUnitario = produto.PrecoEfetivo;
                alterado = true;
            }
        }

        if (alterado)
        {
            if (carrinhoConta != null)
            {
                await _context.SaveChangesAsync();
            }
            else
            {
                SalvarCarrinhoDaSessao(carrinhoSessao!);
            }
        }

        return alterado;
    }

    public async Task AdicionarProdutoAsync(int produtoId, int quantidade, int? usuarioId)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");
        }

        var produto = await _context.Produtos.FirstOrDefaultAsync(p =>
            p.Id == produtoId && p.Status == "Ativo");
        if (produto == null)
        {
            throw new InvalidOperationException("O produto não está disponível.");
        }

        if (usuarioId is > 0)
        {
            var cart = await ObterOuCriarCarrinhoAsync(usuarioId.Value);
            var item = cart.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            var quantidadeSolicitada = (item?.Quantidade ?? 0) + quantidade;
            if (quantidadeSolicitada > produto.Estoque)
            {
                throw new InvalidOperationException("A quantidade solicitada ultrapassa o estoque disponível.");
            }

            if (item == null)
            {
                cart.Itens.Add(new ItemCarrinho
                {
                    ProdutoId = produto.Id,
                    NomeProduto = produto.Nome,
                    Quantidade = quantidade,
                    PrecoUnitario = produto.PrecoEfetivo
                });
            }
            else
            {
                item.NomeProduto = produto.Nome;
                item.PrecoUnitario = produto.PrecoEfetivo;
                item.Quantidade = quantidadeSolicitada;
            }

            await _context.SaveChangesAsync();
            return;
        }

        var sessionCart = ObterCarrinhoDaSessao();
        var sessionItem = sessionCart.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        var desiredQuantity = (sessionItem?.Quantidade ?? 0) + quantidade;
        if (desiredQuantity > produto.Estoque)
        {
            throw new InvalidOperationException("A quantidade solicitada ultrapassa o estoque disponível.");
        }

        if (sessionItem == null)
        {
            sessionCart.Itens.Add(new ItemCarrinho
            {
                ProdutoId = produto.Id,
                NomeProduto = produto.Nome,
                Quantidade = quantidade,
                PrecoUnitario = produto.PrecoEfetivo
            });
        }
        else
        {
            sessionItem.NomeProduto = produto.Nome;
            sessionItem.PrecoUnitario = produto.PrecoEfetivo;
            sessionItem.Quantidade = desiredQuantity;
        }

        SalvarCarrinhoDaSessao(sessionCart);
    }

    public async Task AtualizarQuantidadeAsync(int produtoId, int quantidade, int? usuarioId)
    {
        if (usuarioId is > 0)
        {
            var cart = await ObterCarrinhoAsync(usuarioId.Value);
            var item = cart?.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item == null)
            {
                return;
            }

            if (quantidade <= 0)
            {
                _context.ItensCarrinho.Remove(item);
            }
            else
            {
                var product = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == produtoId && p.Status == "Ativo");
                if (product == null || quantidade > product.Estoque)
                {
                    throw new InvalidOperationException("A quantidade solicitada ultrapassa o estoque disponível.");
                }

                item.Quantidade = quantidade;
                item.NomeProduto = product.Nome;
                item.PrecoUnitario = product.PrecoEfetivo;
            }

            await _context.SaveChangesAsync();
            return;
        }

        var sessionCart = ObterCarrinhoDaSessao();
        var sessionItem = sessionCart.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (sessionItem == null)
        {
            return;
        }

        if (quantidade <= 0)
        {
            sessionCart.Itens.Remove(sessionItem);
        }
        else
        {
            var product = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == produtoId && p.Status == "Ativo");
            if (product == null || quantidade > product.Estoque)
            {
                throw new InvalidOperationException("A quantidade solicitada ultrapassa o estoque disponível.");
            }

            sessionItem.Quantidade = quantidade;
            sessionItem.NomeProduto = product.Nome;
            sessionItem.PrecoUnitario = product.PrecoEfetivo;
        }

        SalvarCarrinhoDaSessao(sessionCart);
    }

    public async Task RemoverProdutoAsync(int produtoId, int? usuarioId)
    {
        if (usuarioId is > 0)
        {
            var cart = await ObterCarrinhoAsync(usuarioId.Value);
            var item = cart?.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
            if (item != null)
            {
                _context.ItensCarrinho.Remove(item);
                await _context.SaveChangesAsync();
            }

            return;
        }

        var sessionCart = ObterCarrinhoDaSessao();
        var sessionItem = sessionCart.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (sessionItem != null)
        {
            sessionCart.Itens.Remove(sessionItem);
            SalvarCarrinhoDaSessao(sessionCart);
        }
    }

    public async Task LimparAsync(int? usuarioId)
    {
        if (usuarioId is > 0)
        {
            var carts = await _context.Carrinhos.Where(c => c.UsuarioId == usuarioId.Value).ToListAsync();
            _context.Carrinhos.RemoveRange(carts);
            await _context.SaveChangesAsync();
        }

        _session.Remove(CarrinhoSessionKey);
    }

    public async Task<int> ObterQuantidadeTotalAsync(int? usuarioId)
    {
        var items = await ObterItensAsync(usuarioId);
        return items.Sum(i => i.Quantidade);
    }

    public async Task<bool> MesclarCarrinhoDaSessaoAsync(int usuarioId)
    {
        var guestCart = ObterCarrinhoDaSessao();
        if (guestCart.Itens.Count == 0)
        {
            return false;
        }

        var accountCart = await ObterOuCriarCarrinhoAsync(usuarioId);
        var stockAdjusted = false;
        foreach (var guestItem in guestCart.Itens)
        {
            var product = await _context.Produtos.FirstOrDefaultAsync(p =>
                p.Id == guestItem.ProdutoId && p.Status == "Ativo");
            if (product == null || product.Estoque == 0)
            {
                stockAdjusted = true;
                continue;
            }

            var accountItem = accountCart.Itens.FirstOrDefault(i => i.ProdutoId == product.Id);
            var requestedQuantity = (accountItem?.Quantidade ?? 0) + guestItem.Quantidade;
            var actualQuantity = Math.Min(requestedQuantity, product.Estoque);
            stockAdjusted |= requestedQuantity != actualQuantity;
            if (accountItem == null)
            {
                accountCart.Itens.Add(new ItemCarrinho
                {
                    ProdutoId = product.Id,
                    NomeProduto = product.Nome,
                    Quantidade = actualQuantity,
                    PrecoUnitario = product.PrecoEfetivo
                });
            }
            else
            {
                accountItem.NomeProduto = product.Nome;
                accountItem.Quantidade = actualQuantity;
                accountItem.PrecoUnitario = product.PrecoEfetivo;
            }
        }

        await _context.SaveChangesAsync();
        _session.Remove(CarrinhoSessionKey);
        return stockAdjusted;
    }

    private async Task<Carrinho?> ObterCarrinhoAsync(int usuarioId) =>
        await _context.Carrinhos.Include(c => c.Itens)
            .Where(c => c.UsuarioId == usuarioId).OrderBy(c => c.Id).FirstOrDefaultAsync();

    private async Task<Carrinho> ObterOuCriarCarrinhoAsync(int usuarioId)
    {
        var cart = await ObterCarrinhoAsync(usuarioId);
        if (cart != null)
        {
            return cart;
        }

        cart = new Carrinho { UsuarioId = usuarioId, DataCriacao = DateTime.UtcNow };
        _context.Carrinhos.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    private Carrinho ObterCarrinhoDaSessao()
    {
        var json = _session.GetString(CarrinhoSessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? new Carrinho()
            : JsonSerializer.Deserialize<Carrinho>(json) ?? new Carrinho();
    }

    private void SalvarCarrinhoDaSessao(Carrinho cart) =>
        _session.SetString(CarrinhoSessionKey, JsonSerializer.Serialize(cart));
}
