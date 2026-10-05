using bow.estoque.Data;
using bow.estoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class ProdutosController : Controller
{
    private readonly ApplicationDbContext _context;
    public ProdutosController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Produtos.Include(p => p.Categoria).Include(p => p.Fornecedor).AsQueryable();
        if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Nome.Contains(search) || p.SKU.Contains(search) || p.CodigoBarras.Contains(search));
        var produtos = await query.OrderBy(p => p.Nome).ToListAsync();
        return View(produtos);
    }

    public async Task<IActionResult> Create(){ await CarregarViewBags(); return View(); }
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Create(Produto produto){ if (!ModelState.IsValid){ await CarregarViewBags(); return View(produto); } produto.DataCadastro = DateTime.Now; produto.DataAtualizacao = DateTime.Now; _context.Add(produto); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Produto cadastrado com sucesso."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Edit(int id){var produto = await _context.Produtos.FindAsync(id); if (produto == null) return NotFound(); await CarregarViewBags(); return View(produto);} 
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id, Produto produto){ if (id != produto.Id) return BadRequest(); if (!ModelState.IsValid){ await CarregarViewBags(); return View(produto);} var produtoDb = await _context.Produtos.FindAsync(id); if (produtoDb == null) return NotFound(); produtoDb.Nome = produto.Nome; produtoDb.Descricao = produto.Descricao; produtoDb.SKU = produto.SKU; produtoDb.CodigoBarras = produto.CodigoBarras; produtoDb.CategoriaId = produto.CategoriaId; produtoDb.FornecedorId = produto.FornecedorId; produtoDb.PrecoCusto = produto.PrecoCusto; produtoDb.PrecoVenda = produto.PrecoVenda; produtoDb.QuantidadeEstoque = produto.QuantidadeEstoque; produtoDb.EstoqueMinimo = produto.EstoqueMinimo; produtoDb.EstoqueMaximo = produto.EstoqueMaximo; produtoDb.Imagem = produto.Imagem; produtoDb.Status = produto.Status; produtoDb.DataAtualizacao = DateTime.Now; await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Produto atualizado."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Details(int id){var produto = await _context.Produtos.Include(p => p.Categoria).Include(p => p.Fornecedor).FirstOrDefaultAsync(p => p.Id == id); if (produto == null) return NotFound(); return View(produto);} 
    public async Task<IActionResult> Delete(int id){var produto = await _context.Produtos.FindAsync(id); if (produto == null) return NotFound(); return View(produto);} 
    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken] public async Task<IActionResult> DeleteConfirmed(int id){var produto = await _context.Produtos.FindAsync(id); if (produto == null) return NotFound(); _context.Produtos.Remove(produto); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Produto removido."; return RedirectToAction(nameof(Index)); }

    private async Task CarregarViewBags(){ ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(); ViewBag.Fornecedores = await _context.Fornecedores.OrderBy(f => f.NomeFantasia).ToListAsync(); }
}
