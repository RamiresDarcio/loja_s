using loja_s.Data;
using loja_s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Controllers;

public class PagamentoController : Controller
{
    private readonly ApplicationDbContext _context;

    public PagamentoController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Pix(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Pagamento)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        ViewBag.CodigoPix = pedido.Pagamento?.CodigoReferencia ?? "pix-felibow-simulado";
        ViewBag.Valor = pedido.ValorTotal;
        return View(pedido);
    }

    [HttpGet]
    public async Task<IActionResult> Cartao(int id)
    {
        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null)
        {
            return NotFound();
        }

        return View(pedido);
    }

    [HttpGet]
    public async Task<IActionResult> Boleto(int id)
    {
        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null)
        {
            return NotFound();
        }

        return View(pedido);
    }
}
