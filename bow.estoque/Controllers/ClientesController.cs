using bow.estoque.Data;
using bow.estoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly ApplicationDbContext _context;
    public ClientesController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Clientes.AsQueryable();
        if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.Nome.Contains(search) || c.CPF.Contains(search) || c.Email.Contains(search));
        var clientes = await query.OrderBy(c => c.Nome).ToListAsync();
        return View(clientes);
    }

    public IActionResult Create() => View();
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Create(Cliente cliente){ if(!ModelState.IsValid) return View(cliente); cliente.DataCadastro = DateTime.Now; _context.Add(cliente); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Cliente cadastrado com sucesso."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Edit(int id){var cliente = await _context.Clientes.FindAsync(id); if (cliente == null) return NotFound(); return View(cliente);} 
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id, Cliente cliente){ if (id != cliente.Id) return BadRequest(); if (!ModelState.IsValid) return View(cliente); var clienteDb = await _context.Clientes.FindAsync(id); if (clienteDb == null) return NotFound(); _context.Entry(clienteDb).CurrentValues.SetValues(cliente); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Cliente atualizado."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Details(int id){ var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id); if (cliente == null) return NotFound(); var historico = await _context.Vendas.Include(v => v.Itens).ThenInclude(i => i.Produto).Where(v => v.ClienteId == id).OrderByDescending(v => v.Data).ToListAsync(); ViewBag.HistoricoCompras = historico; return View(cliente); }
    public async Task<IActionResult> Delete(int id){var cliente = await _context.Clientes.FindAsync(id); if (cliente == null) return NotFound(); return View(cliente);} 
    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken] public async Task<IActionResult> DeleteConfirmed(int id){var cliente = await _context.Clientes.FindAsync(id); if (cliente == null) return NotFound(); _context.Clientes.Remove(cliente); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Cliente removido."; return RedirectToAction(nameof(Index)); }
}
