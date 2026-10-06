using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using loja_s.Models;
using loja_s.Data;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q, string? ordenar)
    {
        var produtosQuery = _context.Produtos.AsNoTracking()
            .Where(p => p.Status == "Ativo");
        if (!string.IsNullOrWhiteSpace(q))
        {
            var termo = q.Trim();
            produtosQuery = produtosQuery.Where(p =>
                p.Nome.Contains(termo) || p.Descricao.Contains(termo) || p.SKU.Contains(termo));
        }

        produtosQuery = ordenar switch
        {
            "menor-preco" => produtosQuery.OrderBy(p =>
                p.PrecoPromocional.HasValue && p.PrecoPromocional.Value > 0 && p.PrecoPromocional.Value < p.Preco
                    ? p.PrecoPromocional.Value
                    : p.Preco),
            "maior-preco" => produtosQuery.OrderByDescending(p =>
                p.PrecoPromocional.HasValue && p.PrecoPromocional.Value > 0 && p.PrecoPromocional.Value < p.Preco
                    ? p.PrecoPromocional.Value
                    : p.Preco),
            _ => produtosQuery.OrderBy(p => p.Nome)
        };

        ViewBag.Busca = q;
        ViewBag.Ordenacao = ordenar;
        return View(await produtosQuery.ToListAsync());
    }

    public IActionResult Cria()
    {
        return RedirectToAction("Cadastro", "Conta");
    }

    public IActionResult Login()
    {
        return RedirectToAction("Login", "Conta");
    }

    public IActionResult PaginalP()
    {
        return View("paginal_p");
    }

    [HttpGet]
    public async Task<IActionResult> VerProduto(int id)
    {
        var produto = await _context.Produtos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == "Ativo");
        if (produto == null)
        {
            return NotFound();
        }

        ViewBag.Relacionados = await _context.Produtos.AsNoTracking()
            .Where(p => p.Id != id && p.Status == "Ativo")
            .OrderBy(p => p.Nome)
            .Take(4)
            .ToListAsync();
        return View(produto);
    }

    public Task<IActionResult> Produto_1() => ExibirProduto(1);

    public Task<IActionResult> Produto_2() => ExibirProduto(2);

    public Task<IActionResult> Produto_3() => ExibirProduto(3);

    public Task<IActionResult> Produto_4() => ExibirProduto(4);

    public Task<IActionResult> Produto_5() => ExibirProduto(5);

    public Task<IActionResult> Produto_6() => ExibirProduto(6);

    public Task<IActionResult> Produto_7() => ExibirProduto(7);

    public Task<IActionResult> Produto_8() => ExibirProduto(8);

    private async Task<IActionResult> ExibirProduto(int numero)
    {
        if (numero is < 1 or > 8)
        {
            return NotFound();
        }

        var produto = await _context.Produtos.AsNoTracking()
            .Where(p => p.Status == "Ativo")
            .OrderBy(p => p.Id)
            .Skip(numero - 1)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
        return produto == 0
            ? NotFound()
            : RedirectToAction(nameof(VerProduto), new { id = produto });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
