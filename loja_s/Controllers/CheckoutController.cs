using loja_s.Data;
using loja_s.Models;
using loja_s.Services;
using loja_s.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace loja_s.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CarrinhoService _carrinhoService;
    private readonly PedidoService _pedidoService;

    public CheckoutController(ApplicationDbContext context, CarrinhoService carrinhoService, PedidoService pedidoService)
    {
        _context = context;
        _carrinhoService = carrinhoService;
        _pedidoService = pedidoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarioId = UsuarioIdAtual();
        ViewBag.CatalogoAtualizado = await _carrinhoService.SincronizarComCatalogoAsync(usuarioId);
        var itens = await _carrinhoService.ObterItensAsync(usuarioId);
        if (!itens.Any())
        {
            return RedirectToAction("Index", "Carrinho");
        }

        var subtotal = itens.Sum(i => i.Quantidade * i.PrecoUnitario);
        var frete = subtotal > 0 ? 19.90m : 0m;
        var usuario = await _context.Usuarios.Include(u => u.PerfilConta)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
        {
            return Challenge();
        }

        var endereco = await _context.Enderecos.Where(e => e.UsuarioId == usuarioId)
            .OrderByDescending(e => e.Principal).FirstOrDefaultAsync();
        var model = new CheckoutViewModel
        {
            NomeCompleto = $"{usuario.Nome} {usuario.PerfilConta?.Sobrenome}".Trim(),
            CPF = usuario.CPF,
            Email = usuario.Email,
            Telefone = usuario.Telefone ?? string.Empty,
            CEP = endereco?.CEP ?? string.Empty,
            Rua = endereco?.Rua ?? string.Empty,
            Numero = endereco?.Numero ?? string.Empty,
            Complemento = endereco?.Complemento,
            Bairro = endereco?.Bairro ?? string.Empty,
            Cidade = endereco?.Cidade ?? string.Empty,
            Estado = endereco?.Estado ?? string.Empty,
            Subtotal = subtotal,
            Frete = frete,
            ValorTotal = subtotal + frete,
            Itens = itens.Select(i => new ItemCarrinhoResumo
            {
                ProdutoId = i.ProdutoId,
                Nome = i.NomeProduto,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario,
                Subtotal = i.Subtotal
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        if (await _carrinhoService.SincronizarComCatalogoAsync(UsuarioIdAtual()))
        {
            TempData["CheckoutCatalogoAtualizado"] =
                "O preço ou a disponibilidade de um item mudou. Revise o resumo atualizado antes de concluir a compra.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var itens = await _carrinhoService.ObterItensAsync(UsuarioIdAtual());
        if (!itens.Any())
        {
            TempData["ErrorMessage"] = "Seu carrinho está vazio.";
            return RedirectToAction("Index", "Carrinho");
        }

        var usuarioId = UsuarioIdAtual();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
        {
            return Challenge();
        }

        if (!new[] { "PIX", "CartaoCredito", "CartaoDebito", "Boleto" }
                .Contains(model.FormaPagamento, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.FormaPagamento), "Selecione uma forma de pagamento válida.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            model.NomeCompleto = usuario.Nome;
            model.Email = usuario.Email;
            model.Desconto = 0m;
            usuario.CPF = model.CPF.Trim();
            usuario.Telefone = model.Telefone.Trim();
            await _context.SaveChangesAsync();
            var pedido = await _pedidoService.CriarPedidoAsync(model, itens, usuarioId);
            await _carrinhoService.LimparAsync(usuarioId);
            return RedirectToAction("Confirmacao", new { id = pedido.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirmacao(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .Include(p => p.EnderecoEntrega)
            .Include(p => p.Pagamento)
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
