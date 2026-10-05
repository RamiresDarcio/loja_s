from pathlib import Path

root = Path(r"C:\Laboratório de Programação\site_loja_s\bow.estoque")
files = {
    "appsettings.json": '''{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=bow_estoque;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
''',
    "bow.estoque.csproj": '''<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0-preview.7.25380.108" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0-preview.7.25380.108">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
''',
    "Program.cs": '''using bow.estoque.Data;
using bow.estoque.Models;
using bow.estoque.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=bow_estoque;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "bow.estoque.auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    SeedAdminUser(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

static void SeedAdminUser(ApplicationDbContext context)
{
    if (context.Usuarios.Any())
    {
        return;
    }

    var usuario = new Usuario
    {
        NomeUsuario = "adm2020",
        NomeCompleto = "Administrador do Sistema",
        Email = "adm@bowestoque.com",
        SenhaHash = PasswordHashing.Hash("adm12345"),
        Status = "Ativo",
        DataCadastro = DateTime.Now
    };

    context.Usuarios.Add(usuario);
    context.SaveChanges();
}
''',
    "Services/PasswordHashing.cs": '''using System.Security.Cryptography;
using System.Text;

namespace bow.estoque.Services;

public static class PasswordHashing
{
    public static string Hash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool Verify(string password, string hash)
    {
        return string.Equals(Hash(password), hash, StringComparison.OrdinalIgnoreCase);
    }
}
''',
    "Models/Usuario.cs": '''using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class Usuario
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string NomeUsuario { get; set; } = string.Empty;
    [Required, StringLength(150)] public string NomeCompleto { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string SenhaHash { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public DateTime DataCadastro { get; set; } = DateTime.Now;
}
''',
    "Models/Categoria.cs": '''using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class Categoria
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = string.Empty;
    [StringLength(500)] public string Descricao { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
''',
    "Models/Fornecedor.cs": '''using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class Fornecedor
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string RazaoSocial { get; set; } = string.Empty;
    [StringLength(200)] public string NomeFantasia { get; set; } = string.Empty;
    [Required, StringLength(20)] public string CNPJ { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [Phone] public string Telefone { get; set; } = string.Empty;
    [StringLength(20)] public string CEP { get; set; } = string.Empty;
    [StringLength(100)] public string Estado { get; set; } = string.Empty;
    [StringLength(150)] public string Cidade { get; set; } = string.Empty;
    [StringLength(250)] public string Endereco { get; set; } = string.Empty;
    [StringLength(20)] public string Numero { get; set; } = string.Empty;
    [StringLength(200)] public string Complemento { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
''',
    "Models/Cliente.cs": '''using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class Cliente
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Nome { get; set; } = string.Empty;
    [Required, StringLength(20)] public string CPF { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [Phone] public string Telefone { get; set; } = string.Empty;
    [StringLength(20)] public string CEP { get; set; } = string.Empty;
    [StringLength(100)] public string Estado { get; set; } = string.Empty;
    [StringLength(150)] public string Cidade { get; set; } = string.Empty;
    [StringLength(250)] public string Endereco { get; set; } = string.Empty;
    [StringLength(20)] public string Numero { get; set; } = string.Empty;
    [StringLength(200)] public string Complemento { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
}
''',
    "Models/Produto.cs": '''using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bow.estoque.Models;

public class Produto
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Nome { get; set; } = string.Empty;
    [StringLength(1000)] public string Descricao { get; set; } = string.Empty;
    [Required, StringLength(50)] public string SKU { get; set; } = string.Empty;
    [StringLength(50)] public string CodigoBarras { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public int FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PrecoCusto { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PrecoVenda { get; set; }
    public int QuantidadeEstoque { get; set; }
    public int EstoqueMinimo { get; set; }
    public int EstoqueMaximo { get; set; }
    [StringLength(300)] public string Imagem { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
}
''',
    "Models/MovimentacaoEstoque.cs": '''using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    [Required, StringLength(30)] public string TipoMovimentacao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int EstoqueAnterior { get; set; }
    public int EstoqueAtual { get; set; }
    [StringLength(500)] public string Motivo { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
}
''',
    "Models/Venda.cs": '''using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bow.estoque.Models;

public class Venda
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
    [StringLength(20)] public string Status { get; set; } = "Pendente";
    [Column(TypeName = "decimal(18,2)")] public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Desconto { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Frete { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Total { get; set; }
    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}
''',
    "Models/ItemVenda.cs": '''using System.ComponentModel.DataAnnotations.Schema;

namespace bow.estoque.Models;

public class ItemVenda
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public Venda? Venda { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public int Quantidade { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PrecoUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Subtotal { get; set; }
}
''',
    "Data/ApplicationDbContext.cs": '''using bow.estoque.Models;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<ItemVenda> ItensVenda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(u => u.NomeUsuario).IsUnique();
        modelBuilder.Entity<Produto>().HasIndex(p => p.SKU).IsUnique();
        modelBuilder.Entity<Fornecedor>().HasIndex(f => f.CNPJ).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(c => c.CPF).IsUnique();

        modelBuilder.Entity<Produto>().HasOne(p => p.Categoria).WithMany(c => c.Produtos).HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Produto>().HasOne(p => p.Fornecedor).WithMany(f => f.Produtos).HasForeignKey(p => p.FornecedorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MovimentacaoEstoque>().HasOne(m => m.Produto).WithMany(p => p.Movimentacoes).HasForeignKey(m => m.ProdutoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MovimentacaoEstoque>().HasOne(m => m.Usuario).WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Venda>().HasOne(v => v.Cliente).WithMany(c => c.Vendas).HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Venda>().HasOne(v => v.Usuario).WithMany().HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ItemVenda>().HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ItemVenda>().HasOne(i => i.Venda).WithMany(v => v.Itens).HasForeignKey(i => i.VendaId).OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
''',
    "ViewModels/LoginViewModel.cs": '''using System.ComponentModel.DataAnnotations;

namespace bow.estoque.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o usuário.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;
}
''',
    "ViewModels/DashboardViewModel.cs": '''namespace bow.estoque.ViewModels;

public class DashboardViewModel
{
    public int TotalProdutos { get; set; }
    public int ProdutosEmEstoque { get; set; }
    public int EstoqueBaixo { get; set; }
    public int ProdutosSemEstoque { get; set; }
    public int TotalVendas { get; set; }
    public decimal Faturamento { get; set; }
    public List<string> VendasPorMes { get; set; } = new();
    public List<int> QuantidadeVendasPorMes { get; set; } = new();
    public List<string> ProdutosMaisVendidos { get; set; } = new();
    public List<int> QuantidadeVendidaPorProduto { get; set; } = new();
    public List<string> ProdutosEstoque { get; set; } = new();
    public List<int> QuantidadeEstoqueAtual { get; set; } = new();
}
''',
    "ViewModels/VendaCreateViewModel.cs": '''using System.ComponentModel.DataAnnotations;

namespace bow.estoque.ViewModels;

public class VendaCreateViewModel
{
    [Required]
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    [Range(0, 999999)] public decimal Desconto { get; set; }
    [Range(0, 999999)] public decimal Frete { get; set; }
    public List<VendaItemForm> Itens { get; set; } = new();
}

public class VendaItemForm
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
''',
    "Controllers/AccountController.cs": '''using System.Security.Claims;
using bow.estoque.Data;
using bow.estoque.Services;
using bow.estoque.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NomeUsuario == model.Usuario && u.Status == "Ativo");
        if (usuario == null || !PasswordHashing.Verify(model.Senha, usuario.SenhaHash))
        {
            TempData["ErrorMessage"] = "Usuário ou senha inválidos.";
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NomeUsuario),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
''',
    "Controllers/DashboardController.cs": '''using bow.estoque.Data;
using bow.estoque.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        var totalProdutos = await _context.Produtos.CountAsync();
        var produtosEmEstoque = await _context.Produtos.CountAsync(p => p.QuantidadeEstoque > 0);
        var estoqueBaixo = await _context.Produtos.CountAsync(p => p.QuantidadeEstoque > 0 && p.QuantidadeEstoque < p.EstoqueMinimo);
        var produtosSemEstoque = await _context.Produtos.CountAsync(p => p.QuantidadeEstoque == 0);
        var totalVendas = await _context.Vendas.CountAsync(v => v.Status == "Finalizada");
        var faturamento = await _context.Vendas.Where(v => v.Status == "Finalizada").SumAsync(v => (decimal?)v.Total) ?? 0m;

        var vendasPorMes = await _context.Vendas
            .Where(v => v.Data >= DateTime.Now.AddMonths(-5))
            .GroupBy(v => new { v.Data.Year, v.Data.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { Label = g.Key.Month + "/" + g.Key.Year, Total = g.Count() })
            .ToListAsync();

        var produtosMaisVendidos = await _context.ItensVenda
            .GroupBy(i => i.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Total = g.Sum(x => x.Quantidade) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .Join(_context.Produtos, x => x.ProdutoId, p => p.Id, (x, p) => new { Nome = p.Nome, Quantidade = x.Total })
            .ToListAsync();

        var produtosEstoque = await _context.Produtos.OrderBy(p => p.Nome).Take(7).Select(p => new { p.Nome, p.QuantidadeEstoque }).ToListAsync();

        var model = new DashboardViewModel
        {
            TotalProdutos = totalProdutos,
            ProdutosEmEstoque = produtosEmEstoque,
            EstoqueBaixo = estoqueBaixo,
            ProdutosSemEstoque = produtosSemEstoque,
            TotalVendas = totalVendas,
            Faturamento = faturamento,
            VendasPorMes = vendasPorMes.Select(v => v.Label).ToList(),
            QuantidadeVendasPorMes = vendasPorMes.Select(v => v.Total).ToList(),
            ProdutosMaisVendidos = produtosMaisVendidos.Select(p => p.Nome).ToList(),
            QuantidadeVendidaPorProduto = produtosMaisVendidos.Select(p => p.Quantidade).ToList(),
            ProdutosEstoque = produtosEstoque.Select(p => p.Nome).ToList(),
            QuantidadeEstoqueAtual = produtosEstoque.Select(p => p.QuantidadeEstoque).ToList()
        };

        return View(model);
    }
}
''',
    "Controllers/CategoriasController.cs": '''using bow.estoque.Data;
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
''',
    "Controllers/FornecedoresController.cs": '''using bow.estoque.Data;
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
''',
    "Controllers/ClientesController.cs": '''using bow.estoque.Data;
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
''',
    "Controllers/ProdutosController.cs": '''using bow.estoque.Data;
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
''',
    "Controllers/EstoqueController.cs": '''using bow.estoque.Data;
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
''',
    "Controllers/HistoricoEstoqueController.cs": '''using bow.estoque.Data;
using bow.estoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
public class HistoricoEstoqueController : Controller
{
    private readonly ApplicationDbContext _context;
    public HistoricoEstoqueController(ApplicationDbContext context) { _context = context; }

    public async Task<IActionResult> Index(int? produtoId, string? tipoMovimentacao, int? usuarioId, DateTime? dataInicial, DateTime? dataFinal)
    {
        var query = _context.MovimentacoesEstoque.Include(m => m.Produto).Include(m => m.Usuario).AsQueryable();
        if (produtoId.HasValue) query = query.Where(m => m.ProdutoId == produtoId.Value);
        if (!string.IsNullOrWhiteSpace(tipoMovimentacao)) query = query.Where(m => m.TipoMovimentacao == tipoMovimentacao);
        if (usuarioId.HasValue) query = query.Where(m => m.UsuarioId == usuarioId.Value);
        if (dataInicial.HasValue) query = query.Where(m => m.Data >= dataInicial.Value);
        if (dataFinal.HasValue) query = query.Where(m => m.Data <= dataFinal.Value.AddDays(1));
        var movimentacoes = await query.OrderByDescending(m => m.Data).ToListAsync();
        ViewBag.Produtos = await _context.Produtos.OrderBy(p => p.Nome).ToListAsync();
        ViewBag.Usuarios = await _context.Usuarios.OrderBy(u => u.NomeUsuario).ToListAsync();
        return View(movimentacoes);
    }
}
''',
    "Controllers/VendasController.cs": '''using bow.estoque.Data;
using bow.estoque.Models;
using bow.estoque.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Controllers;

[Authorize]
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
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Cancelar(int id){ var venda = await _context.Vendas.FindAsync(id); if (venda == null) return NotFound(); venda.Status = "Cancelada"; await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Venda cancelada."; return RedirectToAction(nameof(Index)); }
    private async Task CarregarDadosCreate(){ ViewBag.Clientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync(); ViewBag.Produtos = await _context.Produtos.Where(p => p.Status == "Ativo").OrderBy(p => p.Nome).ToListAsync(); }
    private int ObterUsuarioLogadoId(){ var claim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier); return claim != null ? int.Parse(claim.Value) : 1; }
}
''',
    "Views/_ViewImports.cshtml": '''@using bow.estoque
@using bow.estoque.Models
@using bow.estoque.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
''',
    "Views/_ViewStart.cshtml": '''@{ Layout = "_Layout"; }
''',
    "Views/Shared/_Layout.cshtml": '''<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - bow.estoque</title>
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header class="app-header">
        <div class="container topbar">
            <div class="brand-box">
                <div class="brand-mark">B</div>
                <div>
                    <strong>bow.estoque</strong>
                    <small>Painel administrativo</small>
                </div>
            </div>
            @if (User.Identity?.IsAuthenticated == true)
            {
                <nav class="main-nav" aria-label="Navegação principal">
                    <a asp-controller="Dashboard" asp-action="Index">Dashboard</a>
                    <a asp-controller="Produtos" asp-action="Index">Produtos</a>
                    <a asp-controller="Categorias" asp-action="Index">Categorias</a>
                    <a asp-controller="Fornecedores" asp-action="Index">Fornecedores</a>
                    <a asp-controller="Clientes" asp-action="Index">Clientes</a>
                    <a asp-controller="Estoque" asp-action="Index">Estoque</a>
                    <a asp-controller="HistoricoEstoque" asp-action="Index">Histórico</a>
                    <a asp-controller="Vendas" asp-action="Index">Vendas</a>
                </nav>
                <div class="user-area">
                    <span>Olá, @User.Identity.Name</span>
                    <a asp-controller="Account" asp-action="Logout" class="btn btn-light">Sair</a>
                </div>
            }
        </div>
    </header>

    <main class="app-main">
        @if (TempData["SuccessMessage"] != null)
        {
            <div class="alert success">@TempData["SuccessMessage"]</div>
        }
        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert error">@TempData["ErrorMessage"]</div>
        }
        @RenderBody()
    </main>

    <footer class="app-footer">
        <div class="container"><span>© 2026 bow.estoque</span><span>Sistema de gerenciamento de estoque</span></div>
    </footer>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
''',
    "Views/Shared/Error.cshtml": '''@model ErrorViewModel
@{
    ViewData["Title"] = "Erro";
}
<h1>Ops</h1>
<p>Ocorreu um erro ao processar a solicitação.</p>
''',
    "Views/Account/Login.cshtml": '''@model bow.estoque.ViewModels.LoginViewModel
@{ ViewData["Title"] = "Login"; Layout = null; }
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - bow.estoque</title>
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body class="login-page">
    <div class="login-box">
        <div class="login-header">
            <div class="brand-mark login-mark">B</div>
            <h1>Login</h1>
        </div>
        <form asp-action="Login" method="post" class="form-card">
            <div class="form-group">
                <label asp-for="Usuario">Usuário</label>
                <input asp-for="Usuario" />
                <span asp-validation-for="Usuario" class="field-validation-error"></span>
            </div>
            <div class="form-group">
                <label asp-for="Senha">Senha</label>
                <input asp-for="Senha" type="password" />
                <span asp-validation-for="Senha" class="field-validation-error"></span>
            </div>
            <button type="submit" class="btn primary full">Entrar</button>
        </form>
        <div class="login-note"><strong>Credenciais de desenvolvimento:</strong><br />Usuário: <strong>adm2020</strong><br />Senha: <strong>adm12345</strong></div>
    </div>
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
</body>
</html>
''',
    "Views/Dashboard/Index.cshtml": '''@model bow.estoque.ViewModels.DashboardViewModel
@{ ViewData["Title"] = "Dashboard Administrativo"; }
<div class="page-header"><div><p class="eyebrow">Painel principal</p><h1>Dashboard Administrativo</h1></div></div>
<div class="stats-grid">
    <div class="stat-card"><span>Total de produtos</span><strong>@Model.TotalProdutos</strong></div>
    <div class="stat-card"><span>Produtos em estoque</span><strong>@Model.ProdutosEmEstoque</strong></div>
    <div class="stat-card warning"><span>Estoque baixo</span><strong>@Model.EstoqueBaixo</strong></div>
    <div class="stat-card danger"><span>Produtos sem estoque</span><strong>@Model.ProdutosSemEstoque</strong></div>
    <div class="stat-card neutral"><span>Vendas</span><strong>@Model.TotalVendas</strong></div>
    <div class="stat-card accent"><span>Faturamento</span><strong>R$ @Model.Faturamento.ToString("N2")</strong></div>
</div>
<div class="charts-grid">
    <div class="panel"><h3>Vendas</h3><canvas id="vendasChart" height="160"></canvas></div>
    <div class="panel"><h3>Produtos mais vendidos</h3><canvas id="produtosChart" height="160"></canvas></div>
    <div class="panel"><h3>Estoque</h3><canvas id="estoqueChart" height="160"></canvas></div>
</div>
@section Scripts {
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script>
const vendasLabels = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.VendasPorMes));
const vendasData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.QuantidadeVendasPorMes));
const produtosLabels = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.ProdutosMaisVendidos));
const produtosData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.QuantidadeVendidaPorProduto));
const estoqueLabels = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.ProdutosEstoque));
const estoqueData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model.QuantidadeEstoqueAtual));
new Chart(document.getElementById('vendasChart'), { type: 'line', data: { labels: vendasLabels, datasets: [{ label: 'Vendas', data: vendasData, borderColor: '#5b76f2', backgroundColor: 'rgba(91,118,242,0.18)', fill: true, tension: 0.3 }] }, options: { responsive: true, maintainAspectRatio: false } });
new Chart(document.getElementById('produtosChart'), { type: 'bar', data: { labels: produtosLabels, datasets: [{ label: 'Quantidade vendida', data: produtosData, backgroundColor: '#ff5a5a' }] }, options: { responsive: true, maintainAspectRatio: false } });
new Chart(document.getElementById('estoqueChart'), { type: 'bar', data: { labels: estoqueLabels, datasets: [{ label: 'Estoque atual', data: estoqueData, backgroundColor: '#2bb673' }] }, options: { responsive: true, maintainAspectRatio: false } });
</script>
}
''',
    "Views/Produtos/Index.cshtml": '''@model IEnumerable<bow.estoque.Models.Produto>
@{ ViewData["Title"] = "Produtos"; }
<div class="page-header"><div><p class="eyebrow">Cadastro</p><h1>Produtos</h1></div><a asp-action="Create" class="btn primary">Novo produto</a></div>
<form method="get" class="toolbar"><input type="text" name="search" placeholder="Pesquisar por nome, SKU ou código" value="@Context.Request.Query["search"]" /><button type="submit" class="btn primary">Buscar</button></form>
<div class="table-wrap"><table class="data-table"><thead><tr><th>Id</th><th>Nome</th><th>SKU</th><th>Categoria</th><th>Estoque</th><th>Preço</th><th>Status</th><th>Ações</th></tr></thead><tbody>@foreach (var item in Model){<tr><td>@item.Id</td><td>@item.Nome</td><td>@item.SKU</td><td>@(item.Categoria?.Nome ?? "-")</td><td>@item.QuantidadeEstoque</td><td>R$ @item.PrecoVenda.ToString("N2")</td><td><span class="badge @((item.Status == "Ativo" ? "success" : "muted"))">@item.Status</span></td><td class="actions"><a asp-action="Details" asp-route-id="@item.Id" class="btn small">Detalhes</a><a asp-action="Edit" asp-route-id="@item.Id" class="btn small">Editar</a><a asp-action="Delete" asp-route-id="@item.Id" class="btn danger small">Excluir</a></td></tr>}</tbody></table></div>
''',
    "Views/Produtos/Create.cshtml": '''@model bow.estoque.Models.Produto
@{ ViewData["Title"] = "Novo produto"; }
<div class="page-header"><div><p class="eyebrow">Cadastro</p><h1>Novo produto</h1></div></div>
<form asp-action="Create" method="post" class="panel form-panel"><div class="form-grid"><div class="form-group"><label asp-for="Nome">Nome</label><input asp-for="Nome" /><span asp-validation-for="Nome" class="field-validation-error"></span></div><div class="form-group"><label asp-for="SKU">SKU</label><input asp-for="SKU" /><span asp-validation-for="SKU" class="field-validation-error"></span></div><div class="form-group"><label asp-for="CodigoBarras">Código de barras</label><input asp-for="CodigoBarras" /></div><div class="form-group"><label asp-for="CategoriaId">Categoria</label><select asp-for="CategoriaId" asp-items="@(new SelectList((IEnumerable<bow.estoque.Models.Categoria>)ViewBag.Categorias, "Id", "Nome"))"><option value="">Selecione</option></select></div><div class="form-group"><label asp-for="FornecedorId">Fornecedor</label><select asp-for="FornecedorId" asp-items="@(new SelectList((IEnumerable<bow.estoque.Models.Fornecedor>)ViewBag.Fornecedores, "Id", "NomeFantasia"))"><option value="">Selecione</option></select></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div><div class="form-group"><label asp-for="PrecoCusto">Preço de custo</label><input asp-for="PrecoCusto" type="number" step="0.01" /></div><div class="form-group"><label asp-for="PrecoVenda">Preço de venda</label><input asp-for="PrecoVenda" type="number" step="0.01" /></div><div class="form-group"><label asp-for="QuantidadeEstoque">Quantidade em estoque</label><input asp-for="QuantidadeEstoque" type="number" /></div><div class="form-group"><label asp-for="EstoqueMinimo">Estoque mínimo</label><input asp-for="EstoqueMinimo" type="number" /></div><div class="form-group"><label asp-for="EstoqueMaximo">Estoque máximo</label><input asp-for="EstoqueMaximo" type="number" /></div><div class="form-group full"><label asp-for="Descricao">Descrição</label><textarea asp-for="Descricao"></textarea></div><div class="form-group full"><label asp-for="Imagem">Imagem</label><input asp-for="Imagem" /></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
''',
    "Views/Produtos/Edit.cshtml": '''@model bow.estoque.Models.Produto
@{ ViewData["Title"] = "Editar produto"; }
<div class="page-header"><div><p class="eyebrow">Cadastro</p><h1>Editar produto</h1></div></div>
<form asp-action="Edit" method="post" class="panel form-panel"><input type="hidden" asp-for="Id" /><div class="form-grid"><div class="form-group"><label asp-for="Nome">Nome</label><input asp-for="Nome" /></div><div class="form-group"><label asp-for="SKU">SKU</label><input asp-for="SKU" /></div><div class="form-group"><label asp-for="CodigoBarras">Código de barras</label><input asp-for="CodigoBarras" /></div><div class="form-group"><label asp-for="CategoriaId">Categoria</label><select asp-for="CategoriaId" asp-items="@(new SelectList((IEnumerable<bow.estoque.Models.Categoria>)ViewBag.Categorias, "Id", "Nome"))"></select></div><div class="form-group"><label asp-for="FornecedorId">Fornecedor</label><select asp-for="FornecedorId" asp-items="@(new SelectList((IEnumerable<bow.estoque.Models.Fornecedor>)ViewBag.Fornecedores, "Id", "NomeFantasia"))"></select></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div><div class="form-group"><label asp-for="PrecoCusto">Preço de custo</label><input asp-for="PrecoCusto" type="number" step="0.01" /></div><div class="form-group"><label asp-for="PrecoVenda">Preço de venda</label><input asp-for="PrecoVenda" type="number" step="0.01" /></div><div class="form-group"><label asp-for="QuantidadeEstoque">Quantidade em estoque</label><input asp-for="QuantidadeEstoque" type="number" /></div><div class="form-group"><label asp-for="EstoqueMinimo">Estoque mínimo</label><input asp-for="EstoqueMinimo" type="number" /></div><div class="form-group"><label asp-for="EstoqueMaximo">Estoque máximo</label><input asp-for="EstoqueMaximo" type="number" /></div><div class="form-group full"><label asp-for="Descricao">Descrição</label><textarea asp-for="Descricao"></textarea></div><div class="form-group full"><label asp-for="Imagem">Imagem</label><input asp-for="Imagem" /></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar alterações</button><a asp-action="Index" class="btn">Voltar</a></div></form>
''',
    "Views/Produtos/Details.cshtml": '''@model bow.estoque.Models.Produto
@{ ViewData["Title"] = "Detalhes do produto"; }
<div class="page-header"><div><p class="eyebrow">Visualização</p><h1>@Model.Nome</h1></div></div>
<div class="panel details-panel"><dl class="details-list"><div><dt>Id</dt><dd>@Model.Id</dd></div><div><dt>SKU</dt><dd>@Model.SKU</dd></div><div><dt>Código de barras</dt><dd>@Model.CodigoBarras</dd></div><div><dt>Categoria</dt><dd>@(Model.Categoria?.Nome ?? "-")</dd></div><div><dt>Fornecedor</dt><dd>@(Model.Fornecedor?.NomeFantasia ?? "-")</dd></div><div><dt>Preço de custo</dt><dd>R$ @Model.PrecoCusto.ToString("N2")</dd></div><div><dt>Preço de venda</dt><dd>R$ @Model.PrecoVenda.ToString("N2")</dd></div><div><dt>Estoque</dt><dd>@Model.QuantidadeEstoque</dd></div><div><dt>Estoque mínimo</dt><dd>@Model.EstoqueMinimo</dd></div><div><dt>Estoque máximo</dt><dd>@Model.EstoqueMaximo</dd></div><div><dt>Status</dt><dd>@Model.Status</dd></div><div><dt>Cadastro</dt><dd>@Model.DataCadastro.ToString("dd/MM/yyyy")</dd></div></dl></div>
<div class="actions-row"><a asp-action="Edit" asp-route-id="@Model.Id" class="btn primary">Editar</a><a asp-action="Index" class="btn">Voltar</a></div>
''',
    "Views/Produtos/Delete.cshtml": '''@model bow.estoque.Models.Produto
@{ ViewData["Title"] = "Excluir produto"; }
<div class="panel danger-panel"><h1>Excluir produto</h1><p>Tem certeza que deseja excluir <strong>@Model.Nome</strong>?</p><form asp-action="Delete" method="post"><input type="hidden" asp-for="Id" /><button type="submit" class="btn danger">Confirmar exclusão</button><a asp-action="Index" class="btn">Cancelar</a></form></div>
''',
    "Views/Categorias/Index.cshtml": '''@model IEnumerable<bow.estoque.Models.Categoria>
@{ ViewData["Title"] = "Categorias"; }
<div class="page-header"><div><p class="eyebrow">Cadastro</p><h1>Categorias</h1></div><a asp-action="Create" class="btn primary">Nova categoria</a></div>
<form method="get" class="toolbar"><input type="text" name="search" placeholder="Pesquisar categoria" value="@Context.Request.Query["search"]" /><button type="submit" class="btn primary">Buscar</button></form>
<div class="table-wrap"><table class="data-table"><thead><tr><th>Id</th><th>Nome</th><th>Status</th><th>Ações</th></tr></thead><tbody>@foreach (var item in Model){<tr><td>@item.Id</td><td>@item.Nome</td><td><span class="badge @((item.Status == "Ativo" ? "success" : "muted"))">@item.Status</span></td><td class="actions"><a asp-action="Details" asp-route-id="@item.Id" class="btn small">Detalhes</a><a asp-action="Edit" asp-route-id="@item.Id" class="btn small">Editar</a></td></tr>}</tbody></table></div>
''',
    "Views/Categorias/Create.cshtml": '''@model bow.estoque.Models.Categoria
@{ ViewData["Title"] = "Nova categoria"; }
<form asp-action="Create" method="post" class="panel form-panel"><div class="form-grid"><div class="form-group full"><label asp-for="Nome">Nome</label><input asp-for="Nome" /></div><div class="form-group full"><label asp-for="Descricao">Descrição</label><textarea asp-for="Descricao"></textarea></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
''',
    "Views/Categorias/Edit.cshtml": '''@model bow.estoque.Models.Categoria
@{ ViewData["Title"] = "Editar categoria"; }
<form asp-action="Edit" method="post" class="panel form-panel"><input type="hidden" asp-for="Id" /><div class="form-grid"><div class="form-group full"><label asp-for="Nome">Nome</label><input asp-for="Nome" /></div><div class="form-group full"><label asp-for="Descricao">Descrição</label><textarea asp-for="Descricao"></textarea></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
''',
    "Views/Categorias/Details.cshtml": '''@model bow.estoque.Models.Categoria
@{ ViewData["Title"] = "Detalhes da categoria"; }
<div class="panel details-panel"><h1>@Model.Nome</h1><dl class="details-list"><div><dt>Id</dt><dd>@Model.Id</dd></div><div><dt>Descrição</dt><dd>@Model.Descricao</dd></div><div><dt>Status</dt><dd>@Model.Status</dd></div><div><dt>Data de cadastro</dt><dd>@Model.DataCadastro.ToString("dd/MM/yyyy")</dd></div></dl></div>
''',
    "Views/Fornecedores/Index.cshtml": '''@model IEnumerable<bow.estoque.Models.Fornecedor>
@{ ViewData["Title"] = "Fornecedores"; }
<div class="page-header"><div><p class="eyebrow">Cadastro</p><h1>Fornecedores</h1></div><a asp-action="Create" class="btn primary">Novo fornecedor</a></div>
<form method="get" class="toolbar"><input type="text" name="search" value="@Context.Request.Query["search"]" placeholder="Pesquisar fornecedor" /><button type="submit" class="btn primary">Buscar</button></form>
<table class="data-table"><thead><tr><th>Id</th><th>Razão social</th><th>Fantasia</th><th>CNPJ</th><th>Status</th><th>Ações</th></tr></thead><tbody>@foreach (var item in Model){<tr><td>@item.Id</td><td>@item.RazaoSocial</td><td>@item.NomeFantasia</td><td>@item.CNPJ</td><td><span class="badge @((item.Status == "Ativo" ? "success" : "muted"))">@item.Status</span></td><td class="actions"><a asp-action="Details" asp-route-id="@item.Id" class="btn small">Detalhes</a> <a asp-action="Edit" asp-route-id="@item.Id" class="btn small">Editar</a></td></tr>}</tbody></table>
''',
    "Views/Fornecedores/Create.cshtml": '''@model bow.estoque.Models.Fornecedor
@{ ViewData["Title"] = "Novo fornecedor"; }
<form asp-action="Create" class="panel form-panel" method="post"><div class="form-grid"><div class="form-group"><label asp-for="RazaoSocial">Razão Social</label><input asp-for="RazaoSocial" /></div><div class="form-group"><label asp-for="NomeFantasia">Nome Fantasia</label><input asp-for="NomeFantasia" /></div><div class="form-group"><label asp-for="CNPJ">CNPJ</label><input asp-for="CNPJ" /></div><div class="form-group"><label asp-for="Email">Email</label><input asp-for="Email" /></div><div class="form-group"><label asp-for="Telefone">Telefone</label><input asp-for="Telefone" /></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div><div class="form-group"><label asp-for="CEP">CEP</label><input asp-for="CEP" /></div><div class="form-group"><label asp-for="Estado">Estado</label><input asp-for="Estado" /></div><div class="form-group"><label asp-for="Cidade">Cidade</label><input asp-for="Cidade" /></div><div class="form-group full"><label asp-for="Endereco">Endereço</label><input asp-for="Endereco" /></div><div class="form-group"><label asp-for="Numero">Número</label><input asp-for="Numero" /></div><div class="form-group"><label asp-for="Complemento">Complemento</label><input asp-for="Complemento" /></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
''',
    "Views/Fornecedores/Edit.cshtml": '''@model bow.estoque.Models.Fornecedor
@{ ViewData["Title"] = "Editar fornecedor"; }
<form asp-action="Edit" class="panel form-panel" method="post"><input type="hidden" asp-for="Id" /><div class="form-grid"><div class="form-group"><label asp-for="RazaoSocial">Razão Social</label><input asp-for="RazaoSocial" /></div><div class="form-group"><label asp-for="NomeFantasia">Nome Fantasia</label><input asp-for="NomeFantasia" /></div><div class="form-group"><label asp-for="CNPJ">CNPJ</label><input asp-for="CNPJ" /></div><div class="form-group"><label asp-for="Email">Email</label><input asp-for="Email" /></div><div class="form-group"><label asp-for="Telefone">Telefone</label><input asp-for="Telefone" /></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div><div class="form-group"><label asp-for="CEP">CEP</label><input asp-for="CEP" /></div><div class="form-group"><label asp-for="Estado">Estado</label><input asp-for="Estado" /></div><div class="form-group"><label asp-for="Cidade">Cidade</label><input asp-for="Cidade" /></div><div class="form-group full"><label asp-for="Endereco">Endereço</label><input asp-for="Endereco" /></div><div class="form-group"><label asp-for="Numero">Número</label><input asp-for="Numero" /></div><div class="form-group"><label asp-for="Complemento">Complemento</label><input asp-for="Complemento" /></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
''',
    "Views/Fornecedores/Details.cshtml": '''@model bow.estoque.Models.Fornecedor
@{ ViewData["Title"] = "Detalhes do fornecedor"; }
<div class="panel details-panel"><h1>@Model.NomeFantasia</h1><dl class="details-list"><div><dt>Razão social</dt><dd>@Model.RazaoSocial</dd></div><div><dt>CNPJ</dt><dd>@Model.CNPJ</dd></div><div><dt>Email</dt><dd>@Model.Email</dd></div><div><dt>Telefone</dt><dd>@Model.Telefone</dd></div><div><dt>Endereço</dt><dd>@Model.Endereco, @Model.Numero - @Model.Cidade/@Model.Estado</dd></div><div><dt>Status</dt><dd>@Model.Status</dd></div></dl></div>
''',
    "Views/Clientes/Index.cshtml": '''@model IEnumerable<bow.estoque.Models.Cliente>
@{ ViewData["Title"] = "Clientes"; }
<div class="page-header"><div><p class="eyebrow">Cadastro</p><h1>Clientes</h1></div><a asp-action="Create" class="btn primary">Novo cliente</a></div>
<form method="get" class="toolbar"><input type="text" name="search" value="@Context.Request.Query["search"]" placeholder="Pesquisar cliente" /><button type="submit" class="btn primary">Buscar</button></form>
<table class="data-table"><thead><tr><th>Id</th><th>Nome</th><th>CPF</th><th>Email</th><th>Status</th><th>Ações</th></tr></thead><tbody>@foreach(var item in Model){<tr><td>@item.Id</td><td>@item.Nome</td><td>@item.CPF</td><td>@item.Email</td><td><span class="badge @((item.Status == "Ativo" ? "success" : "muted"))">@item.Status</span></td><td class="actions"><a asp-action="Details" asp-route-id="@item.Id" class="btn small">Detalhes</a> <a asp-action="Edit" asp-route-id="@item.Id" class="btn small">Editar</a></td></tr>}</tbody></table>
''',
    "Views/Clientes/Create.cshtml": '''@model bow.estoque.Models.Cliente
@{ ViewData["Title"] = "Novo cliente"; }
<form asp-action="Create" method="post" class="panel form-panel"><div class="form-grid"><div class="form-group"><label asp-for="Nome">Nome</label><input asp-for="Nome" /></div><div class="form-group"><label asp-for="CPF">CPF</label><input asp-for="CPF" /></div><div class="form-group"><label asp-for="Email">Email</label><input asp-for="Email" /></div><div class="form-group"><label asp-for="Telefone">Telefone</label><input asp-for="Telefone" /></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div><div class="form-group"><label asp-for="CEP">CEP</label><input asp-for="CEP" /></div><div class="form-group"><label asp-for="Estado">Estado</label><input asp-for="Estado" /></div><div class="form-group"><label asp-for="Cidade">Cidade</label><input asp-for="Cidade" /></div><div class="form-group full"><label asp-for="Endereco">Endereço</label><input asp-for="Endereco" /></div><div class="form-group"><label asp-for="Numero">Número</label><input asp-for="Numero" /></div><div class="form-group"><label asp-for="Complemento">Complemento</label><input asp-for="Complemento" /></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
''',
    "Views/Clientes/Edit.cshtml": '''@model bow.estoque.Models.Cliente
@{ ViewData["Title"] = "Editar cliente"; }
<form asp-action="Edit" method="post" class="panel form-panel"><input type="hidden" asp-for="Id" /><div class="form-grid"><div class="form-group"><label asp-for="Nome">Nome</label><input asp-for="Nome" /></div><div class="form-group"><label asp-for="CPF">CPF</label><input asp-for="CPF" /></div><div class="form-group"><label asp-for="Email">Email</label><input asp-for="Email" /></div><div class="form-group"><label asp-for="Telefone">Telefone</label><input asp-for="Telefone" /></div><div class="form-group"><label asp-for="Status">Status</label><select asp-for="Status"><option value="Ativo">Ativo</option><option value="Inativo">Inativo</option></select></div><div class="form-group"><label asp-for="CEP">CEP</label><input asp-for="CEP" /></div><div class="form-group"><label asp-for="Estado">Estado</label><input asp-for="Estado" /></div><div class="form-group"><label asp-for="Cidade">Cidade</label><input asp-for="Cidade" /></div><div class="form-group full"><label asp-for="Endereco">Endereço</label><input asp-for="Endereco" /></div><div class="form-group"><label asp-for="Numero">Número</label><input asp-for="Numero" /></div><div class="form-group"><label asp-for="Complemento">Complemento</label><input asp-for="Complemento" /></div></div><div class="actions-row"><button type="submit" class="btn primary">Salvar</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
''',
    "Views/Clientes/Details.cshtml": '''@model bow.estoque.Models.Cliente
@{ ViewData["Title"] = "Detalhes do cliente"; }
<div class="page-header"><div><p class="eyebrow">Cliente</p><h1>@Model.Nome</h1></div></div>
<div class="panel details-panel"><dl class="details-list"><div><dt>CPF</dt><dd>@Model.CPF</dd></div><div><dt>Email</dt><dd>@Model.Email</dd></div><div><dt>Telefone</dt><dd>@Model.Telefone</dd></div><div><dt>Endereço</dt><dd>@Model.Endereco, @Model.Numero - @Model.Cidade/@Model.Estado</dd></div><div><dt>Status</dt><dd>@Model.Status</dd></div></dl></div>
<h2>Histórico de compras</h2>
<table class="data-table"><thead><tr><th>Venda</th><th>Data</th><th>Total</th><th>Status</th></tr></thead><tbody>@foreach(var item in (IEnumerable<bow.estoque.Models.Venda>)ViewBag.HistoricoCompras){<tr><td>@item.Id</td><td>@item.Data.ToString("dd/MM/yyyy")</td><td>R$ @item.Total.ToString("N2")</td><td>@item.Status</td></tr>}</tbody></table>
''',
    "Views/Estoque/Index.cshtml": '''@{ ViewData["Title"] = "Estoque"; var itens = (IEnumerable<dynamic>)ViewBag.Itens; }
<div class="page-header"><div><p class="eyebrow">Controle</p><h1>Gestão de estoque</h1></div></div>
<table class="data-table"><thead><tr><th>Produto</th><th>SKU</th><th>Estoque atual</th><th>Estoque mínimo</th><th>Estoque máximo</th><th>Status</th><th>Movimentação</th></tr></thead><tbody>@foreach (var item in itens){ var produto = (bow.estoque.Models.Produto)item.Produto; <tr><td>@produto.Nome</td><td>@produto.SKU</td><td>@produto.QuantidadeEstoque</td><td>@produto.EstoqueMinimo</td><td>@produto.EstoqueMaximo</td><td><span class="badge @((item.StatusEstoque.ToString() == "ESTOQUE NORMAL" ? "success" : (item.StatusEstoque.ToString() == "ESTOQUE BAIXO" ? "warning" : "danger")))">@item.StatusEstoque</span></td><td><form asp-action="RegistrarMovimentacao" method="post" class="inline-form"><input type="hidden" name="produtoId" value="@produto.Id" /><select name="tipoMovimentacao"><option value="Entrada">Entrada</option><option value="Saida">Saída</option><option value="Ajuste">Ajuste</option></select><input type="number" name="quantidade" min="1" value="1" /><input type="text" name="motivo" placeholder="Motivo" /><button type="submit" class="btn primary small">Salvar</button></form></td></tr>}</tbody></table>
''',
    "Views/HistoricoEstoque/Index.cshtml": '''@model IEnumerable<bow.estoque.Models.MovimentacaoEstoque>
@{ ViewData["Title"] = "Histórico do estoque"; }
<div class="page-header"><div><p class="eyebrow">Movimentações</p><h1>Histórico do estoque</h1></div></div>
<form method="get" class="toolbar"><select name="produtoId"><option value="">Produto</option>@foreach(var p in (IEnumerable<bow.estoque.Models.Produto>)ViewBag.Produtos){<option value="@p.Id">@p.Nome</option>}</select><select name="tipoMovimentacao"><option value="">Tipo</option><option value="Entrada">Entrada</option><option value="Saida">Saída</option><option value="Ajuste">Ajuste</option></select><select name="usuarioId"><option value="">Usuário</option>@foreach(var u in (IEnumerable<bow.estoque.Models.Usuario>)ViewBag.Usuarios){<option value="@u.Id">@u.NomeUsuario</option>}</select><input type="date" name="dataInicial" /><input type="date" name="dataFinal" /><button type="submit" class="btn primary">Filtrar</button></form>
<table class="data-table"><thead><tr><th>Produto</th><th>Tipo</th><th>Quantidade</th><th>Anterior</th><th>Atual</th><th>Usuário</th><th>Data</th><th>Motivo</th></tr></thead><tbody>@foreach(var item in Model){<tr><td>@item.Produto?.Nome</td><td>@item.TipoMovimentacao</td><td>@item.Quantidade</td><td>@item.EstoqueAnterior</td><td>@item.EstoqueAtual</td><td>@item.Usuario?.NomeUsuario</td><td>@item.Data.ToString("dd/MM/yyyy")</td><td>@item.Motivo</td></tr>}</tbody></table>
''',
    "Views/Vendas/Index.cshtml": '''@model IEnumerable<bow.estoque.Models.Venda>
@{ ViewData["Title"] = "Vendas"; }
<div class="page-header"><div><p class="eyebrow">Financeiro</p><h1>Vendas</h1></div><a asp-action="Create" class="btn primary">Nova venda</a></div>
<form method="get" class="toolbar"><input type="date" name="dataInicial" /><input type="date" name="dataFinal" /><select name="status"><option value="">Status</option><option value="Finalizada">Finalizada</option><option value="Cancelada">Cancelada</option></select><button type="submit" class="btn primary">Filtrar</button></form>
<table class="data-table"><thead><tr><th>#</th><th>Cliente</th><th>Data</th><th>Usuário</th><th>Total</th><th>Status</th><th>Ações</th></tr></thead><tbody>@foreach(var item in Model){<tr><td>@item.Id</td><td>@item.Cliente?.Nome</td><td>@item.Data.ToString("dd/MM/yyyy")</td><td>@item.Usuario?.NomeUsuario</td><td>R$ @item.Total.ToString("N2")</td><td><span class="badge @((item.Status == "Finalizada" ? "success" : (item.Status == "Cancelada" ? "danger" : "muted")))">@item.Status</span></td><td class="actions"><a asp-action="Details" asp-route-id="@item.Id" class="btn small">Detalhes</a></td></tr>}</tbody></table>
''',
    "Views/Vendas/Create.cshtml": '''@model bow.estoque.ViewModels.VendaCreateViewModel
@{ ViewData["Title"] = "Nova venda"; }
<div class="page-header"><div><p class="eyebrow">Financeiro</p><h1>Nova venda</h1></div></div>
<form asp-action="Create" method="post" class="panel form-panel"><div class="form-grid"><div class="form-group"><label>Cliente</label><select asp-for="ClienteId"><option value="">Selecione</option>@foreach (var cliente in (IEnumerable<bow.estoque.Models.Cliente>)ViewBag.Clientes){<option value="@cliente.Id">@cliente.Nome</option>}</select></div><div class="form-group"><label>Desconto</label><input asp-for="Desconto" type="number" step="0.01" /></div><div class="form-group"><label>Frete</label><input asp-for="Frete" type="number" step="0.01" /></div></div><div id="itens-venda"><h3>Produtos</h3><div class="linha-item"><select name="Itens[0].ProdutoId"><option value="">Selecione</option>@foreach (var produto in (IEnumerable<bow.estoque.Models.Produto>)ViewBag.Produtos){<option value="@produto.Id">@produto.Nome</option>}</select><input type="number" name="Itens[0].Quantidade" min="1" value="1" /></div></div><div class="actions-row"><button type="button" id="add-item" class="btn">Adicionar produto</button><button type="submit" class="btn primary">Finalizar venda</button><a asp-action="Index" class="btn">Cancelar</a></div></form>
@section Scripts { <script> let index = 1; document.getElementById('add-item').addEventListener('click', () => { const container = document.getElementById('itens-venda'); const row = document.createElement('div'); row.className = 'linha-item'; row.innerHTML = ` <select name="Itens[${index}].ProdutoId"><option value="">Selecione</option>@foreach (var produto in (IEnumerable<bow.estoque.Models.Produto>)ViewBag.Produtos){<option value="@produto.Id">@produto.Nome</option>}</select><input type="number" name="Itens[${index}].Quantidade" min="1" value="1" />`; container.appendChild(row); index++; }); </script> }
''',
    "Views/Vendas/Details.cshtml": '''@model bow.estoque.Models.Venda
@{ ViewData["Title"] = "Detalhes da venda"; }
<div class="panel details-panel"><h1>Venda @Model.Id</h1><dl class="details-list"><div><dt>Cliente</dt><dd>@Model.Cliente?.Nome</dd></div><div><dt>Usuário</dt><dd>@Model.Usuario?.NomeUsuario</dd></div><div><dt>Data</dt><dd>@Model.Data.ToString("dd/MM/yyyy")</dd></div><div><dt>Status</dt><dd>@Model.Status</dd></div><div><dt>Subtotal</dt><dd>R$ @Model.Subtotal.ToString("N2")</dd></div><div><dt>Desconto</dt><dd>R$ @Model.Desconto.ToString("N2")</dd></div><div><dt>Frete</dt><dd>R$ @Model.Frete.ToString("N2")</dd></div><div><dt>Total</dt><dd>R$ @Model.Total.ToString("N2")</dd></div></dl></div>
<table class="data-table"><thead><tr><th>Produto</th><th>Quantidade</th><th>Preço unitário</th><th>Subtotal</th></tr></thead><tbody>@foreach (var item in Model.Itens){<tr><td>@item.Produto?.Nome</td><td>@item.Quantidade</td><td>R$ @item.PrecoUnitario.ToString("N2")</td><td>R$ @item.Subtotal.ToString("N2")</td></tr>}</tbody></table>
''',
    "wwwroot/css/site.css": ''' :root { --cor-principal: #5b76f2; --cor-primaria-escura: #2c3d8d; --cor-fundo: #f4f6fb; --cor-card: #ffffff; --cor-texto: #1d2438; --cor-muted: #68728b; --cor-success: #2bb673; --cor-warning: #f0b429; --cor-danger: #ff4d4d; --cor-border: #e5eaf4; --shadow: 0 12px 28px rgba(21, 32, 71, 0.08); }
* { box-sizing: border-box; }
html { font-size: 14px; }
body { margin: 0; background: var(--cor-fundo); color: var(--cor-texto); font-family: 'Segoe UI', sans-serif; }
a { color: inherit; }
.container { width: min(1200px, calc(100% - 32px)); margin: 0 auto; }
.app-header { background: rgba(255,255,255,0.92); border-bottom: 1px solid var(--cor-border); box-shadow: var(--shadow); position: sticky; top: 0; z-index: 10; }
.topbar { display: flex; justify-content: space-between; align-items: center; gap: 20px; min-height: 78px; }
.brand-box { display: flex; align-items: center; gap: 12px; }
.brand-mark { width: 48px; height: 48px; display: grid; place-items: center; border-radius: 14px; background: linear-gradient(140deg, #7389ff, #4b64e5); box-shadow: 0 8px 16px rgba(91,118,242,0.28); color: white; font-size: 26px; font-weight: 800; }
.brand-box strong { display: block; font-size: 18px; letter-spacing: 0.08em; }
.brand-box small { color: var(--cor-muted); font-size: 11px; letter-spacing: 0.08em; text-transform: uppercase; }
.main-nav { display: flex; flex-wrap: wrap; justify-content: center; gap: 10px; }
.main-nav a { padding: 8px 12px; border-radius: 999px; text-decoration: none; color: #3a4566; font-weight: 700; text-transform: uppercase; font-size: 11px; letter-spacing: 0.06em; }
.main-nav a:hover { background: #eef3ff; color: var(--cor-principal); }
.user-area { display: flex; align-items: center; gap: 12px; color: var(--cor-muted); font-size: 13px; }
.app-main { width: min(1200px, calc(100% - 32px)); margin: 28px auto 40px; }
.page-header { display: flex; justify-content: space-between; align-items: center; gap: 16px; margin-bottom: 20px; }
.eyebrow { margin: 0 0 6px; color: var(--cor-principal); font-size: 11px; font-weight: 800; letter-spacing: 0.12em; text-transform: uppercase; }
h1, h2, h3 { margin: 0 0 10px; }
.btn { display: inline-flex; align-items: center; justify-content: center; min-height: 42px; padding: 0 16px; border: 1px solid var(--cor-border); border-radius: 10px; background: #fff; color: var(--cor-texto); text-decoration: none; cursor: pointer; font-weight: 700; transition: all 0.15s ease; }
.btn:hover { transform: translateY(-1px); }
.btn.primary { background: var(--cor-principal); border-color: var(--cor-principal); color: white; }
.btn.danger { background: var(--cor-danger); border-color: var(--cor-danger); color: white; }
.btn.small { min-height: 32px; padding: 0 12px; font-size: 12px; }
.btn.full { width: 100%; }
.form-card, .panel { background: var(--cor-card); border: 1px solid var(--cor-border); border-radius: 18px; box-shadow: var(--shadow); }
.form-panel { padding: 22px; }
.form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 18px; }
.form-group { display: flex; flex-direction: column; gap: 8px; }
.form-group.full { grid-column: 1 / -1; }
label { font-weight: 700; color: #37415d; }
input, select, textarea { width: 100%; min-height: 42px; padding: 10px 12px; border: 1px solid var(--cor-border); border-radius: 10px; background: #fafbff; color: var(--cor-texto); font: inherit; }
textarea { min-height: 120px; resize: vertical; }
.field-validation-error { color: var(--cor-danger); font-size: 12px; }
.toolbar { display: flex; flex-wrap: wrap; gap: 12px; margin-bottom: 18px; padding: 18px; border: 1px solid var(--cor-border); border-radius: 14px; background: var(--cor-card); box-shadow: var(--shadow); }
.toolbar input, .toolbar select { width: auto; min-width: 180px; flex: 1 1 180px; }
.data-table { width: 100%; border-collapse: collapse; background: var(--cor-card); overflow: hidden; box-shadow: var(--shadow); }
.data-table th, .data-table td { padding: 14px 16px; border-bottom: 1px solid var(--cor-border); text-align: left; }
.data-table thead th { background: #f2f5ff; color: #32405e; font-size: 12px; letter-spacing: 0.08em; text-transform: uppercase; }
.data-table tbody tr:hover { background: #fafbff; }
.badge { display: inline-flex; align-items: center; min-height: 28px; padding: 0 10px; border-radius: 999px; font-size: 11px; font-weight: 800; letter-spacing: 0.05em; text-transform: uppercase; }
.badge.success { background: rgba(43,182,115,0.12); color: #198a56; }
.badge.warning { background: rgba(240,180,41,0.12); color: #b57800; }
.badge.danger { background: rgba(255,77,77,0.12); color: #ce2d2d; }
.badge.muted { background: rgba(104,114,139,0.12); color: #58657e; }
.actions { display: flex; gap: 8px; flex-wrap: wrap; }
.actions-row { margin-top: 20px; display: flex; gap: 12px; align-items: center; flex-wrap: wrap; }
.stats-grid { display: grid; grid-template-columns: repeat(6, minmax(0, 1fr)); gap: 18px; margin-bottom: 24px; }
.stat-card { background: var(--cor-card); border: 1px solid var(--cor-border); border-radius: 18px; box-shadow: var(--shadow); padding: 18px 16px; display: flex; flex-direction: column; gap: 8px; }
.stat-card span { color: var(--cor-muted); font-size: 12px; text-transform: uppercase; letter-spacing: 0.08em; }
.stat-card strong { font-size: clamp(22px,2vw,32px); }
.stat-card.warning { border-color: rgba(240,180,41,0.3); }
.stat-card.danger { border-color: rgba(255,77,77,0.25); }
.stat-card.accent { background: linear-gradient(135deg, #eef3ff, #fff); }
.charts-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 18px; }
canvas { width: 100% !important; max-height: 240px; }
.details-list { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; margin: 0; }
.details-list div { display: flex; flex-direction: column; gap: 6px; padding: 12px 14px; border: 1px solid var(--cor-border); border-radius: 12px; background: #fafbff; }
.details-list dt { color: var(--cor-muted); font-size: 12px; font-weight: 700; text-transform: uppercase; }
.details-list dd { margin: 0; font-size: 15px; font-weight: 600; }
.login-page { min-height: 100vh; display: grid; place-items: center; background: linear-gradient(135deg, #eef2ff, #f4f6fb); }
.login-box { width: min(500px, calc(100% - 32px)); padding: 28px; background: rgba(255,255,255,0.92); border: 1px solid var(--cor-border); border-radius: 24px; box-shadow: var(--shadow); }
.login-header { text-align: center; margin-bottom: 22px; }
.login-mark { margin: 0 auto 14px; }
.login-note { margin-top: 18px; padding: 14px; border-radius: 12px; background: #f4f7ff; color: #3e4d76; line-height: 1.7; }
.alert { padding: 12px 16px; border-radius: 12px; margin-bottom: 18px; font-weight: 600; }
.alert.success { background: rgba(43,182,115,0.12); color: #198a56; }
.alert.error { background: rgba(255,77,77,0.12); color: #ce2d2d; }
.inline-form { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.inline-form input, .inline-form select { min-width: 90px; }
@media (max-width: 980px) { .stats-grid { grid-template-columns: repeat(3, minmax(0, 1fr)); } .charts-grid { grid-template-columns: 1fr; } }
@media (max-width: 760px) { .topbar { flex-direction: column; align-items: flex-start; padding: 16px 0; } .main-nav { justify-content: flex-start; } .form-grid, .details-list, .stats-grid { grid-template-columns: 1fr; } .page-header { flex-direction: column; align-items: flex-start; } }
''',
}

for relative_path, content in files.items():
    path = root / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding='utf-8')

print(f"Arquivos criados: {len(files)}")
