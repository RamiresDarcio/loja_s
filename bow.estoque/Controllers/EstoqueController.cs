using bow.estoque.Data;
using bow.estoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class EstoqueController : Controller
{
    private readonly ApplicationDbContext _context;
    public EstoqueController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var produtos = await _context.Produtos.Include(p => p.Categoria).OrderBy(p => p.Nome).ToListAsync();
        var lista = produtos.Select(p => new { Produto = p, StatusEstoque = ObterStatusEstoque(p.QuantidadeEstoque, p.EstoqueMinimo) }).ToList();
        ViewBag.Itens = lista; ViewBag.Produtos = produtos; return View();
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarMovimentacao(int produtoId, string tipoMovimentacao, int quantidade, string motivo)
    {
        if (produtoId <= 0 || quantidade <= 0)
        {
            TempData["ErrorMessage"] = "Informe um produto e uma quantidade válida.";
            return RedirectToAction(nameof(Index));
        }

        var produto = await _context.Produtos.FindAsync(produtoId);
        if (produto == null){ TempData["ErrorMessage"] = "Produto não encontrado."; return RedirectToAction(nameof(Index)); }

        var usuarioId = ObterUsuarioLogadoId();
        var estoqueAnterior = produto.QuantidadeEstoque;
        var novoEstoque = produto.QuantidadeEstoque;

        switch (tipoMovimentacao)
        {
            case "Entrada": novoEstoque = produto.QuantidadeEstoque + quantidade; break;
            case "Saida": novoEstoque = produto.QuantidadeEstoque - quantidade; if (novoEstoque < 0){ TempData["ErrorMessage"] = "Quantidade insuficiente em estoque."; return RedirectToAction(nameof(Index)); } break;
            case "Ajuste": novoEstoque = quantidade; break;
            default: TempData["ErrorMessage"] = "Tipo de movimentação inválido."; return RedirectToAction(nameof(Index));
        }

        produto.QuantidadeEstoque = novoEstoque; produto.DataAtualizacao = DateTime.Now;
        var movimentacao = new MovimentacaoEstoque { ProdutoId = produtoId, TipoMovimentacao = tipoMovimentacao, Quantidade = quantidade, EstoqueAnterior = estoqueAnterior, EstoqueAtual = novoEstoque, Motivo = motivo, UsuarioId = usuarioId, Data = DateTime.Now };
        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Movimentação registrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public static string ObterStatusEstoque(int quantidade, int minimo) { if (quantidade == 0) return "SEM ESTOQUE"; if (quantidade <= minimo) return "ESTOQUE CRÍTICO"; if (quantidade <= minimo * 2) return "ESTOQUE BAIXO"; return "ESTOQUE NORMAL"; }
    private int ObterUsuarioLogadoId(){ var claim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier); return claim != null ? int.Parse(claim.Value) : 1; }
}
