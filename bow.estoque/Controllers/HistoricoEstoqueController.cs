using bow.estoque.Data;
using bow.estoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class HistoricoEstoqueController : Controller
{
    private readonly ApplicationDbContext _context;
    public HistoricoEstoqueController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(int? produtoId, string? tipoMovimentacao, int? usuarioId, DateTime? dataInicial, DateTime? dataFinal)
    {
        var query = _context.MovimentacoesEstoque.Include(m => m.Produto).Include(m => m.Usuario).AsQueryable();
        if (produtoId.HasValue) query = query.Where(m => m.ProdutoId == produtoId.Value);
        if (!string.IsNullOrWhiteSpace(tipoMovimentacao)) query = query.Where(m => m.TipoMovimentacao == tipoMovimentacao);
        if (usuarioId.HasValue) query = query.Where(m => m.UsuarioId == usuarioId.Value);
        if (dataInicial.HasValue) query = query.Where(m => m.Data >= dataInicial.Value);
        if (dataFinal.HasValue) query = query.Where(m => m.Data <= dataFinal.Value.AddDays(1));
        var movimentacoes = await query.OrderByDescending(m => m.Data).ToListAsync();
        ViewBag.Produtos = await _context.Produtos.OrderBy(p => p.Nome).ToListAsync();
        ViewBag.Usuarios = await _context.Usuarios.OrderBy(u => u.NomeUsuario).ToListAsync();
        return View(movimentacoes);
    }
}
