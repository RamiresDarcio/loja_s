using loja_s.Data;
using loja_s.Models;

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
        var produto = await _context.Produtos.FindAsync(produtoId);
        if (produto == null || produto.Estoque < quantidade)
        {
            return false;
        }

        var estoqueAnterior = produto.Estoque;
        produto.Estoque -= quantidade;

        _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
        {
            ProdutoId = produtoId,
            TipoMovimentacao = "Saida",
            Quantidade = quantidade,
            EstoqueAnterior = estoqueAnterior,
            EstoqueAtual = produto.Estoque,
            Motivo = motivo,
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }
}
