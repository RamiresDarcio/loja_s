using bow.estoque.Data;
using bow.estoque.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var totalProdutos = await _context.Produtos.CountAsync();
        var produtosEmEstoque = await _context.Produtos.CountAsync(p => p.QuantidadeEstoque > 0);
        var estoqueBaixo = await _context.Produtos.CountAsync(p => p.QuantidadeEstoque > 0 && p.QuantidadeEstoque < p.EstoqueMinimo);
        var produtosSemEstoque = await _context.Produtos.CountAsync(p => p.QuantidadeEstoque == 0);
        var totalVendas = await _context.Vendas.CountAsync(v => v.Status == "Finalizada");
        var faturamento = await _context.Vendas.Where(v => v.Status == "Finalizada").SumAsync(v => (decimal?)v.Total) ?? 0m;

        var vendasPorMes = await _context.Vendas
            .Where(v => v.Data >= DateTime.Now.AddMonths(-5))
            .GroupBy(v => new { v.Data.Year, v.Data.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { Label = g.Key.Month + "/" + g.Key.Year, Total = g.Count() })
            .ToListAsync();

        var produtosMaisVendidos = await _context.ItensVenda
            .GroupBy(i => i.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Total = g.Sum(x => x.Quantidade) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .Join(_context.Produtos, x => x.ProdutoId, p => p.Id, (x, p) => new { Nome = p.Nome, Quantidade = x.Total })
            .ToListAsync();

        var produtosEstoque = await _context.Produtos.OrderBy(p => p.Nome).Take(7).Select(p => new { p.Nome, p.QuantidadeEstoque }).ToListAsync();

        var model = new DashboardViewModel
        {
            TotalProdutos = totalProdutos,
            ProdutosEmEstoque = produtosEmEstoque,
            EstoqueBaixo = estoqueBaixo,
            ProdutosSemEstoque = produtosSemEstoque,
            TotalVendas = totalVendas,
            Faturamento = faturamento,
            VendasPorMes = vendasPorMes.Select(v => v.Label).ToList(),
            QuantidadeVendasPorMes = vendasPorMes.Select(v => v.Total).ToList(),
            ProdutosMaisVendidos = produtosMaisVendidos.Select(p => p.Nome).ToList(),
            QuantidadeVendidaPorProduto = produtosMaisVendidos.Select(p => p.Quantidade).ToList(),
            ProdutosEstoque = produtosEstoque.Select(p => p.Nome).ToList(),
            QuantidadeEstoqueAtual = produtosEstoque.Select(p => p.QuantidadeEstoque).ToList()
        };

        return View(model);
    }
}
