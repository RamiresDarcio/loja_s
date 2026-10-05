using loja_s.Data;
using loja_s.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace loja_s.Controllers;

[Authorize]
public class PedidoController : Controller
{
    private readonly ApplicationDbContext _context;

    public PedidoController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> MeusPedidos()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Itens)
            .Include(p => p.Pagamento)
            .Include(p => p.EnderecoEntrega)
            .Where(p => p.UsuarioId == UsuarioIdAtual())
            .OrderByDescending(p => p.DataPedido)
            .ToListAsync();

        return View(pedidos);
    }

    public async Task<IActionResult> Detalhes(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .Include(p => p.Pagamento)
            .Include(p => p.EnderecoEntrega)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == UsuarioIdAtual());

        if (pedido == null)
        {
            return NotFound();
        }

        return View(pedido);
    }

    private int UsuarioIdAtual() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}
