using loja_s.Data;
using loja_s.Models;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Services;

public class EstoqueService
{
    private readonly ApplicationDbContext _context;

    public EstoqueService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> DebitarEstoqueAsync(int produtoId, int quantidade, int usuarioId, string motivo)
    {
        if (quantidade <= 0)
        {
            return false;
        }

        var produto = await _context.Produtos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.Status == "Ativo");
        if (produto == null || produto.Estoque < quantidade)
        {
            return false;
        }

        var estoqueAnterior = produto.Estoque;
        var estoqueAtual = estoqueAnterior - quantidade;
        var linhasAtualizadas = await _context.Produtos
            .Where(p => p.Id == produtoId && p.Status == "Ativo" && p.Estoque >= quantidade)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Estoque, p => p.Estoque - quantidade));
        if (linhasAtualizadas == 0)
        {
            return false;
        }

        _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
        {
            ProdutoId = produtoId,
            TipoMovimentacao = "Saida",
            Quantidade = quantidade,
            EstoqueAnterior = estoqueAnterior,
            EstoqueAtual = estoqueAtual,
            Motivo = motivo,
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }
}
