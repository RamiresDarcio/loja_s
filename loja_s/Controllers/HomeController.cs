using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using loja_s.Models;

namespace loja_s.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Cria()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult PaginalP()
    {
        return View("paginal_p");
    }

    public IActionResult Produto()
    {
        var produto = new Produto
        {
            Nome = "Whey Protein",
            Preco = 0,
            Descricao = "Suplemento utilizado para complementar a alimentação com proteínas."
        };

        return View("produto", produto);
    }

    public IActionResult Produto_1() => ExibirProduto(1);

    public IActionResult Produto_2() => ExibirProduto(2);

    public IActionResult Produto_3() => ExibirProduto(3);

    public IActionResult Produto_4() => ExibirProduto(4);

    public IActionResult Produto_5() => ExibirProduto(5);

    public IActionResult Produto_6() => ExibirProduto(6);

    public IActionResult Produto_7() => ExibirProduto(7);

    public IActionResult Produto_8() => ExibirProduto(8);

    private IActionResult ExibirProduto(int numero)
    {
        var produto = new Produto
        {
            Nome = $"Produto {numero}",
            Preco = 0,
            Descricao = "Suplemento utilizado para complementar a alimentação."
        };

        return View($"produto_{numero}", produto);
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
