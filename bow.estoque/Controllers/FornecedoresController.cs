using bow.estoque.Data;
using bow.estoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class FornecedoresController : Controller
{
    private readonly ApplicationDbContext _context;
    public FornecedoresController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Fornecedores.AsQueryable();
        if (!string.IsNullOrEmpty(search)) query = query.Where(f => f.RazaoSocial.Contains(search) || f.NomeFantasia.Contains(search) || f.CNPJ.Contains(search));
        var fornecedores = await query.OrderBy(f => f.RazaoSocial).ToListAsync();
        return View(fornecedores);
    }

    public IActionResult Create() => View();
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Create(Fornecedor fornecedor) { if (!ModelState.IsValid) return View(fornecedor); _context.Add(fornecedor); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Fornecedor cadastrado com sucesso."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Edit(int id){var fornecedor = await _context.Fornecedores.FindAsync(id); if (fornecedor == null) return NotFound(); return View(fornecedor);} 
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id, Fornecedor fornecedor){if (id != fornecedor.Id) return BadRequest(); if (!ModelState.IsValid) return View(fornecedor); var fornecedorDb = await _context.Fornecedores.FindAsync(id); if (fornecedorDb == null) return NotFound(); _context.Entry(fornecedorDb).CurrentValues.SetValues(fornecedor); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Fornecedor atualizado."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Details(int id){var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(f => f.Id == id); if (fornecedor == null) return NotFound(); return View(fornecedor);}
    public async Task<IActionResult> Delete(int id){var fornecedor = await _context.Fornecedores.FindAsync(id); if (fornecedor == null) return NotFound(); return View(fornecedor); }
    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken] public async Task<IActionResult> DeleteConfirmed(int id){var fornecedor = await _context.Fornecedores.FindAsync(id); if (fornecedor == null) return NotFound(); _context.Fornecedores.Remove(fornecedor); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Fornecedor removido."; return RedirectToAction(nameof(Index)); }
}
