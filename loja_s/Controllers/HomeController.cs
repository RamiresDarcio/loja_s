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
        var produto = numero switch
        {
            1 => new Produto
            {
                Nome = "Power Dragon Creatine — Bowsette Edition",
                Descricao = "Creatina monohidratada apresentada em uma edição temática inspirada em Bowsette.",
                ImagemUrl = "~/img/produtos/produtos_1.png"
            },
            2 => new Produto
            {
                Nome = "Dark Warrior Whey — Baiken Edition",
                Descricao = "Whey protein em uma edição temática inspirada em guerreiros e desempenho.",
                ImagemUrl = "~/img/produtos/produtos_8.png"
            },
            3 => new Produto
            {
                Nome = "Chaos Energy Multi — Juri Edition",
                Descricao = "Multivitamínico com identidade visual energética inspirada em Juri.",
                ImagemUrl = "~/img/produtos/produtos_3.png"
            },
            4 => new Produto
            {
                Nome = "Blue Sea Omega 3 — Nami Edition",
                Descricao = "Ômega 3 em uma edição temática marítima inspirada em Nami.",
                ImagemUrl = "~/img/banner/nani edition.png"
            },
            5 => new Produto
            {
                Nome = "Mystic Balance Magnesium — Mystique Edition",
                Descricao = "Magnésio em uma edição temática misteriosa e sofisticada inspirada em Mystique.",
                ImagemUrl = "~/img/produtos/produtos_7.png"
            },
            6 => new Produto
            {
                Nome = "Street Power D3 + K2 — CJ Edition",
                Descricao = "Vitaminas D3 e K2 em uma edição temática urbana inspirada em CJ.",
                ImagemUrl = "~/img/produtos/produtos_2.png"
            },
            7 => new Produto
            {
                Nome = "Mystery Recovery Glutamine — Scooby-Doo Edition",
                Descricao = "Glutamina em uma edição divertida com identidade visual inspirada em Scooby-Doo.",
                ImagemUrl = "~/img/produtos/produtos_5.png"
            },
            8 => new Produto
            {
                Nome = "Produto 8 — Scooby-Doo Edition",
                Descricao = "Produto temático da coleção Scooby-Doo. A imagem e os detalhes deste produto serão adicionados quando estiverem disponíveis."
            },
            _ => throw new ArgumentOutOfRangeException(nameof(numero), numero, "Produto inexistente.")
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
