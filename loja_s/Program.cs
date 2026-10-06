using loja_s.Data;
using loja_s.Models;
using loja_s.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var shopConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=felibow_integrated.db";
shopConnectionString = Felibow.Integration.SqliteConnectionStringResolver.Resolve(
    shopConnectionString, builder.Environment.ContentRootPath);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "felibow.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(shopConnectionString));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login";
        options.AccessDeniedPath = "/Conta/Login";
        options.Cookie.Name = "felibow.customer";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnValidatePrincipal = async context =>
        {
            var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var stamp = context.Principal?.FindFirstValue("AccountSecurityStamp");
            var sessionId = context.Principal?.FindFirstValue("AccountSessionId");
            if (!int.TryParse(userIdValue, out var userId) ||
                string.IsNullOrWhiteSpace(stamp) ||
                string.IsNullOrWhiteSpace(sessionId))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var accountSession = await db.SessoesConta
                .Include(s => s.Usuario)
                .ThenInclude(u => u.SegurancaConta)
                .FirstOrDefaultAsync(s => s.UsuarioId == userId && s.ChaveSessao == sessionId);
            if (accountSession?.Usuario.Status != "Ativo" ||
                accountSession.Usuario.SegurancaConta?.SecurityStamp != stamp)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var now = DateTime.UtcNow;
            if (accountSession.UltimaAtividade <= now.AddMinutes(-5))
            {
                accountSession.UltimaAtividade = now;
                await db.SaveChangesAsync();
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IContaEmailService, ContaEmailService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<IPagamentoService, PagamentoSimuladoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<CarrinhoService>(services =>
    new CarrinhoService(
        services.GetRequiredService<IHttpContextAccessor>().HttpContext!.Session,
        services.GetRequiredService<ApplicationDbContext>()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EnsureShopSchemaAsync(db);
    await EnsureProductsTableAsync(db);
    await ContaSchema.EnsureCreatedAsync(db);
    await Felibow.Integration.IntegratedDatabaseImporter.ImportAvailableSourcesAsync(
        db,
        Path.Combine(builder.Environment.ContentRootPath, "loja_s.db"),
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "bow.estoque", "bow_estoque.db")),
        app.Logger);
    SeedProdutos(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task EnsureShopSchemaAsync(ApplicationDbContext context)
{
    var createScript = context.Database.GenerateCreateScript()
        .Replace("CREATE UNIQUE INDEX ", "CREATE UNIQUE INDEX IF NOT EXISTS ", StringComparison.OrdinalIgnoreCase)
        .Replace("CREATE INDEX ", "CREATE INDEX IF NOT EXISTS ", StringComparison.OrdinalIgnoreCase)
        .Replace("CREATE TABLE ", "CREATE TABLE IF NOT EXISTS ", StringComparison.OrdinalIgnoreCase);
    await context.Database.ExecuteSqlRawAsync(createScript);
}

static async Task EnsureProductsTableAsync(ApplicationDbContext context)
{
    var productsTableExists = await context.Database.SqlQueryRaw<int>(
        """SELECT COUNT(*) AS "Value" FROM sqlite_master WHERE type = 'table' AND name = 'Produtos'""")
        .SingleAsync();
    if (productsTableExists == 0)
    {
        throw new InvalidOperationException(
            $"The SQLite database '{context.Database.GetDbConnection().DataSource}' is missing the required Produtos table after schema initialization.");
    }
}

static void SeedProdutos(ApplicationDbContext context)
{
    var produtosIniciais = new[]
    {
        new Produto { Nome = "Power Dragon Creatine — Bowsette Edition", Preco = 149.90m, Descricao = "Creatina para força e performance.", ImagemUrl = "~/img/produtos/produtos_1.png", SKU = "PD-001", Estoque = 24, Status = "Ativo", EdicaoEspecial = "Bowsette Edition" },
        new Produto { Nome = "Dark Warrior Whey — Baiken Edition", Preco = 169.90m, Descricao = "Whey protein para recuperação muscular.", ImagemUrl = "~/img/produtos/produtos_8.png", SKU = "DW-002", Estoque = 18, Status = "Ativo", EdicaoEspecial = "Baiken Edition" },
        new Produto { Nome = "Chaos Energy Multi — Juri Edition", Preco = 99.90m, Descricao = "Multivitamínico para rotina ativa.", ImagemUrl = "~/img/produtos/produtos_3.png", SKU = "CE-003", Estoque = 20, Status = "Ativo", EdicaoEspecial = "Juri Edition" },
        new Produto { Nome = "Blue Sea Omega 3 — Nami Edition", Preco = 129.90m, Descricao = "Ômega 3 com qualidade premium.", ImagemUrl = "~/img/banner/nani edition.png", SKU = "BS-004", Estoque = 14, Status = "Ativo", EdicaoEspecial = "Nami Edition" },
        new Produto { Nome = "Mystic Balance Magnesium — Mystique Edition", Preco = 89.90m, Descricao = "Magnésio para complementar sua rotina.", ImagemUrl = "~/img/produtos/produtos_7.png", SKU = "MB-005", Estoque = 16, Status = "Ativo", EdicaoEspecial = "Mystique Edition" },
        new Produto { Nome = "Street Power D3 + K2 — CJ Edition", Preco = 79.90m, Descricao = "Vitaminas D3 e K2 para sua rotina.", ImagemUrl = "~/img/produtos/produtos_2.png", SKU = "SP-006", Estoque = 18, Status = "Ativo", EdicaoEspecial = "CJ Edition" },
        new Produto { Nome = "Mystery Recovery Glutamine — Scooby-Doo Edition", Preco = 109.90m, Descricao = "Glutamina para recuperação e rotina esportiva.", ImagemUrl = "~/img/produtos/produtos_5.png", SKU = "MR-007", Estoque = 12, Status = "Ativo", EdicaoEspecial = "Scooby-Doo Edition" },
        new Produto { Nome = "Mystery Gut Balance — Scooby-Doo & Friends Edition", Preco = 119.90m, Descricao = "Produto temático da coleção Scooby-Doo & Friends.", ImagemUrl = string.Empty, SKU = "MGB-008", Estoque = 10, Status = "Ativo", EdicaoEspecial = "Scooby-Doo & Friends Edition" }
    };

    var skusExistentes = context.Produtos.Select(p => p.SKU).ToHashSet(StringComparer.OrdinalIgnoreCase);
    var produtosAusentes = produtosIniciais.Where(p => !skusExistentes.Contains(p.SKU)).ToList();
    if (produtosAusentes.Count > 0)
    {
        context.Produtos.AddRange(produtosAusentes);
        context.SaveChanges();
    }
}
