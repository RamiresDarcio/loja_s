using System.Security.Claims;
using loja_s.Data;
using loja_s.Models;
using loja_s.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Controllers;

[Authorize]
public class EnderecosController : Controller
{
    private readonly ApplicationDbContext _context;

    public EnderecosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarioId = UsuarioIdAtual();
        var enderecos = await _context.Enderecos.Where(e => e.UsuarioId == usuarioId)
            .OrderByDescending(e => e.Principal).ThenBy(e => e.Nome).ToListAsync();
        return View(enderecos);
    }

    [HttpGet]
    public IActionResult Criar() => View(new EnderecoContaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(EnderecoContaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuarioId = UsuarioIdAtual();
        var addressCount = await _context.Enderecos.CountAsync(e => e.UsuarioId == usuarioId);
        var endereco = new Endereco
        {
            UsuarioId = usuarioId,
            Nome = model.Nome.Trim(),
            CEP = model.CEP.Trim(),
            Rua = model.Rua.Trim(),
            Numero = model.Numero.Trim(),
            Complemento = model.Complemento?.Trim(),
            Bairro = model.Bairro.Trim(),
            Cidade = model.Cidade.Trim(),
            Estado = model.Estado.Trim().ToUpperInvariant(),
            Pais = model.Pais.Trim(),
            Principal = model.Principal || addressCount == 0
        };
        if (endereco.Principal)
        {
            await DefinirEnderecoPrincipalAsync(usuarioId, null);
        }

        _context.Enderecos.Add(endereco);
        await _context.SaveChangesAsync();
        TempData["ContaMensagem"] = "Endereço adicionado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var endereco = await ObterEnderecoDoUsuarioAsync(id);
        if (endereco == null)
        {
            return NotFound();
        }

        return View(new EnderecoContaViewModel
        {
            Id = endereco.Id,
            Nome = endereco.Nome,
            CEP = endereco.CEP,
            Rua = endereco.Rua,
            Numero = endereco.Numero,
            Complemento = endereco.Complemento,
            Bairro = endereco.Bairro,
            Cidade = endereco.Cidade,
            Estado = endereco.Estado,
            Pais = endereco.Pais,
            Principal = endereco.Principal
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EnderecoContaViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var endereco = await ObterEnderecoDoUsuarioAsync(id);
        if (endereco == null)
        {
            return NotFound();
        }

        if (model.Principal)
        {
            await DefinirEnderecoPrincipalAsync(endereco.UsuarioId, id);
        }

        AtualizarEndereco(endereco, model);
        await _context.SaveChangesAsync();
        TempData["ContaMensagem"] = "Endereço atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var endereco = await ObterEnderecoDoUsuarioAsync(id);
        if (endereco == null)
        {
            return NotFound();
        }

        if (await _context.Pedidos.AnyAsync(p => p.EnderecoEntregaId == id))
        {
            TempData["ErroConta"] = "Este endereço está associado a um pedido e não pode ser removido.";
            return RedirectToAction(nameof(Index));
        }

        var eraPrincipal = endereco.Principal;
        var usuarioId = endereco.UsuarioId;
        _context.Enderecos.Remove(endereco);
        await _context.SaveChangesAsync();
        if (eraPrincipal)
        {
            var proximo = await _context.Enderecos.Where(e => e.UsuarioId == usuarioId)
                .OrderBy(e => e.Id).FirstOrDefaultAsync();
            if (proximo != null)
            {
                proximo.Principal = true;
                await _context.SaveChangesAsync();
            }
        }

        TempData["ContaMensagem"] = "Endereço removido.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DefinirPrincipal(int id)
    {
        var endereco = await ObterEnderecoDoUsuarioAsync(id);
        if (endereco == null)
        {
            return NotFound();
        }

        await DefinirEnderecoPrincipalAsync(endereco.UsuarioId, id);
        await _context.SaveChangesAsync();
        TempData["ContaMensagem"] = "Endereço principal atualizado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Endereco?> ObterEnderecoDoUsuarioAsync(int id) =>
        await _context.Enderecos.FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == UsuarioIdAtual());

    private async Task DefinirEnderecoPrincipalAsync(int usuarioId, int? enderecoSelecionado)
    {
        var enderecos = await _context.Enderecos.Where(e => e.UsuarioId == usuarioId).ToListAsync();
        foreach (var endereco in enderecos)
        {
            endereco.Principal = enderecoSelecionado.HasValue && endereco.Id == enderecoSelecionado.Value;
        }
    }

    private static void AtualizarEndereco(Endereco endereco, EnderecoContaViewModel model)
    {
        endereco.Nome = model.Nome.Trim();
        endereco.CEP = model.CEP.Trim();
        endereco.Rua = model.Rua.Trim();
        endereco.Numero = model.Numero.Trim();
        endereco.Complemento = model.Complemento?.Trim();
        endereco.Bairro = model.Bairro.Trim();
        endereco.Cidade = model.Cidade.Trim();
        endereco.Estado = model.Estado.Trim().ToUpperInvariant();
        endereco.Pais = model.Pais.Trim();
        endereco.Principal = model.Principal;
    }

    private int UsuarioIdAtual() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
