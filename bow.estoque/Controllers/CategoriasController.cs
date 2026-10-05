using bow.estoque.Data;
using bow.estoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class CategoriasController : Controller
{
    private readonly ApplicationDbContext _context;
    public CategoriasController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Categorias.AsQueryable();
        if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.Nome.Contains(search) || c.Descricao.Contains(search));
        var categorias = await query.OrderBy(c => c.Nome).ToListAsync();
        return View(categorias);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (!ModelState.IsValid) return View(categoria);
        categoria.DataCadastro = DateTime.Now;
        _context.Add(categoria);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Categoria cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) { var categoria = await _context.Categorias.FindAsync(id); if (categoria == null) return NotFound(); return View(categoria); }
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.Id) return BadRequest();
        if (!ModelState.IsValid) return View(categoria);
        var categoriaDb = await _context.Categorias.FindAsync(id);
        if (categoriaDb == null) return NotFound();
        categoriaDb.Nome = categoria.Nome; categoriaDb.Descricao = categoria.Descricao; categoriaDb.Status = categoria.Status;
        await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Categoria atualizada com sucesso."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id){var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id); if (categoria == null) return NotFound(); return View(categoria); }
    public async Task<IActionResult> Delete(int id){var categoria = await _context.Categorias.FindAsync(id); if (categoria == null) return NotFound(); return View(categoria); }
    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id){var categoria = await _context.Categorias.FindAsync(id); if (categoria == null) return NotFound(); _context.Categorias.Remove(categoria); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Categoria removida."; return RedirectToAction(nameof(Index)); }
}
