using bow.estoque.Data;
using bow.estoque.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

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
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count() })
            .ToListAsync();

        var produtosMaisVendidosAdmin = await _context.ItensVenda
            .GroupBy(i => i.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Total = g.Sum(x => x.Quantidade) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .Join(_context.Produtos, x => x.ProdutoId, p => p.Id, (x, p) => new { Nome = p.Nome, Quantidade = x.Total })
            .ToListAsync();

        var monthlySales = vendasPorMes.ToDictionary(
            v => new DateTime(v.Year, v.Month, 1),
            v => v.Total);
        var productSales = produtosMaisVendidosAdmin
            .GroupBy(p => p.Nome)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantidade));

        if (_context.Database.IsSqlite())
        {
            var storefrontTables = await _context.Database.SqlQueryRaw<int>(
                """SELECT COUNT(*) AS "Value" FROM sqlite_master WHERE type = 'table' AND name IN ('Pedidos', 'ItensPedido')""")
                .SingleAsync();

            if (storefrontTables == 2)
            {
                var paidOrders = _context.Database.SqlQueryRaw<DashboardStoreOrderValue>(
                    """
                    SELECT "ValorTotal" AS "ValorTotal"
                    FROM "Pedidos"
                    WHERE "Status" = 'Finalizado' AND "StatusPagamento" = 'Pago'
                    """);
                var storeOrderValues = await paidOrders.ToListAsync();
                totalVendas += storeOrderValues.Count;
                faturamento += storeOrderValues.Sum(order => order.ValorTotal);

                var storeMonthlySales = await _context.Database.SqlQueryRaw<DashboardMonthlySales>(
                    """
                    SELECT strftime('%Y-%m', "DataPedido") AS "Mes", COUNT(*) AS "Total"
                    FROM "Pedidos"
                    WHERE "Status" = 'Finalizado' AND "StatusPagamento" = 'Pago'
                        AND "DataPedido" >= {0}
                    GROUP BY strftime('%Y-%m', "DataPedido")
                    """,
                    DateTime.Now.AddMonths(-5).ToString("yyyy-MM-dd"))
                    .ToListAsync();

                foreach (var month in storeMonthlySales)
                {
                    if (DateTime.TryParseExact(
                        month.Mes,
                        "yyyy-MM",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var parsedMonth))
                    {
                        var key = new DateTime(parsedMonth.Year, parsedMonth.Month, 1);
                        monthlySales[key] = monthlySales.GetValueOrDefault(key) + month.Total;
                    }
                }

                var storeTopProducts = await _context.Database.SqlQueryRaw<DashboardProductSales>(
                    """
                    SELECT i."NomeProduto" AS "Nome", SUM(i."Quantidade") AS "Quantidade"
                    FROM "ItensPedido" AS i
                    INNER JOIN "Pedidos" AS p ON p."Id" = i."PedidoId"
                    WHERE p."Status" = 'Finalizado' AND p."StatusPagamento" = 'Pago'
                    GROUP BY i."NomeProduto"
                    """)
                    .ToListAsync();

                foreach (var product in storeTopProducts)
                {
                    productSales[product.Nome] = productSales.GetValueOrDefault(product.Nome) + product.Quantidade;
                }
            }
            else
            {
                _logger.LogWarning("Storefront order tables are not available; dashboard totals include inventory sales only.");
            }
        }
        else
        {
            _logger.LogWarning("Storefront orders are only included in the dashboard when the shared database uses SQLite.");
        }

        var topProducts = productSales
            .OrderByDescending(product => product.Value)
            .Take(5)
            .ToList();
        var produtosEstoque = await _context.Produtos.OrderBy(p => p.Nome).Take(7).Select(p => new { p.Nome, p.QuantidadeEstoque }).ToListAsync();

        var model = new DashboardViewModel
        {
            TotalProdutos = totalProdutos,
            ProdutosEmEstoque = produtosEmEstoque,
            EstoqueBaixo = estoqueBaixo,
            ProdutosSemEstoque = produtosSemEstoque,
            TotalVendas = totalVendas,
            Faturamento = faturamento,
            VendasPorMes = monthlySales.OrderBy(v => v.Key).Select(v => $"{v.Key.Month}/{v.Key.Year}").ToList(),
            QuantidadeVendasPorMes = monthlySales.OrderBy(v => v.Key).Select(v => v.Value).ToList(),
            ProdutosMaisVendidos = topProducts.Select(p => p.Key).ToList(),
            QuantidadeVendidaPorProduto = topProducts.Select(p => p.Value).ToList(),
            ProdutosEstoque = produtosEstoque.Select(p => p.Nome).ToList(),
            QuantidadeEstoqueAtual = produtosEstoque.Select(p => p.QuantidadeEstoque).ToList()
        };

        return View(model);
    }
}
