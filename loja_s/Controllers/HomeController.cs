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
