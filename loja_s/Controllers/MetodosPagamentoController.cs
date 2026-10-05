using System.Security.Claims;
using loja_s.Data;
using loja_s.Models;
using loja_s.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Controllers;

[Authorize]
public class MetodosPagamentoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDataProtector _tokenProtector;

    public MetodosPagamentoController(ApplicationDbContext context, IDataProtectionProvider protectionProvider)
    {
        _context = context;
        _tokenProtector = protectionProvider.CreateProtector("loja_s.saved-payment-token.v1");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var metodos = await _context.MetodosPagamentoSalvos
            .Where(m => m.UsuarioId == UsuarioIdAtual())
            .OrderByDescending(m => m.Preferencial).ThenByDescending(m => m.DataCadastro)
            .ToListAsync();
        return View(metodos);
    }

    [HttpGet]
    public IActionResult Adicionar() => View(new MetodoPagamentoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(MetodoPagamentoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tokenDigits = new string(model.TokenProvedor.Where(char.IsDigit).ToArray());
        if (tokenDigits.Length is >= 12 and <= 19 &&
            tokenDigits.Length >= model.TokenProvedor.Length * 0.8)
        {
            ModelState.AddModelError(nameof(model.TokenProvedor),
                "Informe a referência tokenizada do gateway, nunca o número do cartão.");
            return View(model);
        }

        var usuarioId = UsuarioIdAtual();
        var firstMethod = !await _context.MetodosPagamentoSalvos.AnyAsync(m => m.UsuarioId == usuarioId);
        _context.MetodosPagamentoSalvos.Add(new MetodoPagamentoSalvo
        {
            UsuarioId = usuarioId,
            Bandeira = model.Bandeira.Trim(),
            UltimosQuatro = model.UltimosQuatro,
            TokenProvedor = _tokenProtector.Protect(model.TokenProvedor.Trim()),
            Preferencial = firstMethod,
            DataCadastro = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        TempData["ContaMensagem"] = "Método tokenizado cadastrado. Nenhum número completo ou código de segurança foi armazenado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(int id)
    {
        var metodo = await _context.MetodosPagamentoSalvos.FirstOrDefaultAsync(m =>
            m.Id == id && m.UsuarioId == UsuarioIdAtual());
        if (metodo == null)
        {
            return NotFound();
        }

        var wasPreferred = metodo.Preferencial;
        var usuarioId = metodo.UsuarioId;
        _context.MetodosPagamentoSalvos.Remove(metodo);
        await _context.SaveChangesAsync();
        if (wasPreferred)
        {
            var nextMethod = await _context.MetodosPagamentoSalvos
                .Where(m => m.UsuarioId == usuarioId).OrderBy(m => m.Id).FirstOrDefaultAsync();
            if (nextMethod != null)
            {
                nextMethod.Preferencial = true;
                await _context.SaveChangesAsync();
            }
        }

        TempData["ContaMensagem"] = "Método de pagamento removido.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DefinirPreferencial(int id)
    {
        var metodo = await _context.MetodosPagamentoSalvos.FirstOrDefaultAsync(m =>
            m.Id == id && m.UsuarioId == UsuarioIdAtual());
        if (metodo == null)
        {
            return NotFound();
        }

        var methods = await _context.MetodosPagamentoSalvos
            .Where(m => m.UsuarioId == metodo.UsuarioId).ToListAsync();
        foreach (var savedMethod in methods)
        {
            savedMethod.Preferencial = savedMethod.Id == id;
        }

        await _context.SaveChangesAsync();
        TempData["ContaMensagem"] = "Método preferencial atualizado.";
        return RedirectToAction(nameof(Index));
    }

    private int UsuarioIdAtual() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
