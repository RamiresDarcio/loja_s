using System.Security.Claims;
using loja_s.Data;
using loja_s.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Controllers;

[Authorize]
public class FavoritosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CarrinhoService _carrinhoService;

    public FavoritosController(ApplicationDbContext context, CarrinhoService carrinhoService)
    {
        _context = context;
        _carrinhoService = carrinhoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var favoritos = await _context.Favoritos.Include(f => f.Produto)
            .Where(f => f.UsuarioId == UsuarioIdAtual())
            .OrderByDescending(f => f.DataAdicionado).ToListAsync();
        return View(favoritos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(int produtoId, string? returnUrl)
    {
        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == produtoId && p.Status == "Ativo");
        if (produto == null)
        {
            return NotFound();
        }

        var usuarioId = UsuarioIdAtual();
        if (!await _context.Favoritos.AnyAsync(f => f.UsuarioId == usuarioId && f.ProdutoId == produtoId))
        {
            _context.Favoritos.Add(new Models.Favorito { UsuarioId = usuarioId, ProdutoId = produtoId });
            await _context.SaveChangesAsync();
        }

        TempData["ContaMensagem"] = "Produto adicionado aos favoritos.";
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(int id)
    {
        var favorito = await _context.Favoritos.FirstOrDefaultAsync(f =>
            f.Id == id && f.UsuarioId == UsuarioIdAtual());
        if (favorito == null)
        {
            return NotFound();
        }

        _context.Favoritos.Remove(favorito);
        await _context.SaveChangesAsync();
        TempData["ContaMensagem"] = "Produto removido dos favoritos.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarAoCarrinho(int id)
    {
        var favorito = await _context.Favoritos.Include(f => f.Produto)
            .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId == UsuarioIdAtual());
        if (favorito?.Produto == null || favorito.Produto.Status != "Ativo")
        {
            return NotFound();
        }

        if (favorito.Produto.Estoque < 1)
        {
            TempData["ErroConta"] = "Este produto está sem estoque no momento.";
            return RedirectToAction(nameof(Index));
        }

        await _carrinhoService.AdicionarProdutoAsync(favorito.ProdutoId, 1, UsuarioIdAtual());
        TempData["ContaMensagem"] = "Produto adicionado ao carrinho.";
        return RedirectToAction("Index", "Carrinho");
    }

    private int UsuarioIdAtual() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
