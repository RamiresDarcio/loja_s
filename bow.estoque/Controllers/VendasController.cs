using bow.estoque.Data;
using bow.estoque.Models;
using bow.estoque.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bow.estoque.Controllers;

[Authorize(Roles = "Admin")]
public class VendasController : Controller
{
    private readonly ApplicationDbContext _context;
    public VendasController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(DateTime? dataInicial, DateTime? dataFinal, int? clienteId, string? status, int? usuarioId)
    {
        var query = _context.Vendas.Include(v => v.Cliente).Include(v => v.Usuario).Include(v => v.Itens).ThenInclude(i => i.Produto).AsQueryable();
        if (dataInicial.HasValue) query = query.Where(v => v.Data >= dataInicial.Value);
        if (dataFinal.HasValue) query = query.Where(v => v.Data <= dataFinal.Value.AddDays(1));
        if (clienteId.HasValue) query = query.Where(v => v.ClienteId == clienteId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(v => v.Status == status);
        if (usuarioId.HasValue) query = query.Where(v => v.UsuarioId == usuarioId.Value);
        var vendas = await query.OrderByDescending(v => v.Data).ToListAsync();
        ViewBag.Clientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync();
        ViewBag.Usuarios = await _context.Usuarios.OrderBy(u => u.NomeUsuario).ToListAsync();
        return View(vendas);
    }

    public async Task<IActionResult> Create(){ await CarregarDadosCreate(); return View(new VendaCreateViewModel()); }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VendaCreateViewModel model)
    {
        if (model.ClienteId <= 0 || model.Itens == null || !model.Itens.Any(i => i.ProdutoId > 0 && i.Quantidade > 0))
        {
            TempData["ErrorMessage"] = "Selecione o cliente e pelo menos um produto válido.";
            await CarregarDadosCreate();
            return View(model);
        }

        var usuarioId = ObterUsuarioLogadoId();
        var venda = new Venda { ClienteId = model.ClienteId, UsuarioId = usuarioId, Status = "Finalizada", Data = DateTime.Now, Desconto = model.Desconto, Frete = model.Frete };
        decimal subtotal = 0m;

        foreach (var itemForm in model.Itens.Where(i => i.ProdutoId > 0 && i.Quantidade > 0))
        {
            var produto = await _context.Produtos.FindAsync(itemForm.ProdutoId);
            if (produto == null) continue;
            if (itemForm.Quantidade > produto.QuantidadeEstoque)
            {
                TempData["ErrorMessage"] = $"Estoque insuficiente para o produto {produto.Nome}.";
                await CarregarDadosCreate();
                return View(model);
            }
            var subtotalItem = produto.PrecoVenda * itemForm.Quantidade;
            subtotal += subtotalItem;
            var item = new ItemVenda { ProdutoId = produto.Id, Quantidade = itemForm.Quantidade, PrecoUnitario = produto.PrecoVenda, Subtotal = subtotalItem };
            venda.Itens.Add(item);
            produto.QuantidadeEstoque -= itemForm.Quantidade;
            produto.DataAtualizacao = DateTime.Now;
            _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque { ProdutoId = produto.Id, TipoMovimentacao = "Saida", Quantidade = itemForm.Quantidade, EstoqueAnterior = produto.QuantidadeEstoque + itemForm.Quantidade, EstoqueAtual = produto.QuantidadeEstoque, Motivo = "Venda finalizada", UsuarioId = usuarioId, Data = DateTime.Now });
        }

        venda.Subtotal = subtotal; venda.Total = subtotal - model.Desconto + model.Frete; _context.Vendas.Add(venda); await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Venda registrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id){ var venda = await _context.Vendas.Include(v => v.Cliente).Include(v => v.Usuario).Include(v => v.Itens).ThenInclude(i => i.Produto).FirstOrDefaultAsync(v => v.Id == id); if (venda == null) return NotFound(); return View(venda); }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var venda = await _context.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venda == null) return NotFound();

        if (venda.Status == "Cancelada")
        {
            TempData["SuccessMessage"] = "Esta venda já estava cancelada.";
            return RedirectToAction(nameof(Index));
        }

        var usuarioId = ObterUsuarioLogadoId();
        var updated = await _context.Vendas
            .Where(v => v.Id == id && v.Status == venda.Status)
            .ExecuteUpdateAsync(setters => setters.SetProperty(v => v.Status, "Cancelada"));
        if (updated == 0)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = "A venda foi alterada por outra operação. Atualize a página e tente novamente.";
            return RedirectToAction(nameof(Index));
        }

        if (venda.Status == "Finalizada")
        {
            foreach (var item in venda.Itens.GroupBy(i => i.ProdutoId).Select(g => new
                     {
                         ProdutoId = g.Key,
                         Quantidade = g.Sum(i => i.Quantidade)
                     }))
            {
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId);
                if (produto == null)
                {
                    throw new InvalidOperationException(
                        $"O produto {item.ProdutoId} da venda {venda.Id} não existe; o cancelamento foi revertido.");
                }

                var estoqueAnterior = produto.QuantidadeEstoque;
                produto.QuantidadeEstoque += item.Quantidade;
                produto.DataAtualizacao = DateTime.Now;
                _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    ProdutoId = produto.Id,
                    TipoMovimentacao = "Entrada",
                    Quantidade = item.Quantidade,
                    EstoqueAnterior = estoqueAnterior,
                    EstoqueAtual = produto.QuantidadeEstoque,
                    Motivo = $"Cancelamento da venda #{venda.Id}",
                    UsuarioId = usuarioId,
                    Data = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["SuccessMessage"] = venda.Status == "Finalizada"
            ? "Venda cancelada e estoque devolvido."
            : "Venda cancelada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarDadosCreate(){ ViewBag.Clientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync(); ViewBag.Produtos = await _context.Produtos.Where(p => p.Status == "Ativo").OrderBy(p => p.Nome).ToListAsync(); }
    private int ObterUsuarioLogadoId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)
            ? usuarioId
            : throw new InvalidOperationException("A sessão administrativa não contém um identificador válido.");
}
