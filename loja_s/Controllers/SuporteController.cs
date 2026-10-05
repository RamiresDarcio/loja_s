using System.Security.Claims;
using loja_s.Data;
using loja_s.Models;
using loja_s.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Controllers;

[Authorize]
public class SuporteController : Controller
{
    private static readonly string[] Categorias =
    [
        "Problema com pedido", "Pagamento", "Entrega", "Produto", "Cancelamento",
        "Reembolso", "Problemas com a conta", "Outros"
    ];

    private readonly ApplicationDbContext _context;

    public SuporteController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Categorias = Categorias;
        var usuarioId = UsuarioIdAtual();
        var solicitacoes = await _context.SolicitacoesSuporte.Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.DataCriacao).ToListAsync();
        return View(solicitacoes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(SuporteContaViewModel model)
    {
        if (!Categorias.Contains(model.Categoria, StringComparer.Ordinal))
        {
            ModelState.AddModelError(nameof(model.Categoria), "Selecione uma categoria válida.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = Categorias;
            var userId = UsuarioIdAtual();
            var requests = await _context.SolicitacoesSuporte.Where(s => s.UsuarioId == userId)
                .OrderByDescending(s => s.DataCriacao).ToListAsync();
            return View("Index", requests);
        }

        _context.SolicitacoesSuporte.Add(new SolicitacaoSuporte
        {
            UsuarioId = UsuarioIdAtual(),
            Categoria = model.Categoria,
            Assunto = model.Assunto.Trim(),
            Mensagem = model.Mensagem.Trim()
        });
        await _context.SaveChangesAsync();
        TempData["ContaMensagem"] = "Sua solicitação foi registrada.";
        return RedirectToAction(nameof(Index));
    }

    private int UsuarioIdAtual() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
