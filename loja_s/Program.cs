using loja_s.Data;
using loja_s.Models;
using loja_s.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

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
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=loja_s.db"));
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
            var valid = await db.Usuarios.AnyAsync(u =>
                u.Id == userId && u.Status == "Ativo" &&
                u.SegurancaConta != null && u.SegurancaConta.SecurityStamp == stamp &&
                db.SessoesConta.Any(s => s.UsuarioId == userId && s.ChaveSessao == sessionId));
            if (!valid)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
    db.Database.EnsureCreated();
    await ContaSchema.EnsureCreatedAsync(db);
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

static void SeedProdutos(ApplicationDbContext context)
{
    if (context.Produtos.Any())
    {
        return;
    }

    context.Produtos.AddRange(
        new Produto { Nome = "Power Dragon Creatine", Preco = 149.90m, Descricao = "Creatina para força e performance.", ImagemUrl = "~/img/produtos/produtos_1.png", SKU = "PD-001", Estoque = 24, Status = "Ativo" },
        new Produto { Nome = "Dark Warrior Whey", Preco = 169.90m, Descricao = "Whey protein para recuperação muscular.", ImagemUrl = "~/img/produtos/produtos_8.png", SKU = "DW-002", Estoque = 18, Status = "Ativo" },
        new Produto { Nome = "Chaos Energy Multi", Preco = 99.90m, Descricao = "Multivitamínico para rotina ativa.", ImagemUrl = "~/img/produtos/produtos_3.png", SKU = "CE-003", Estoque = 20, Status = "Ativo" },
        new Produto { Nome = "Blue Sea Omega 3", Preco = 129.90m, Descricao = "Ômega 3 com qualidade premium.", ImagemUrl = "~/img/banner/nani edition.png", SKU = "BS-004", Estoque = 14, Status = "Ativo" }
    );

    context.SaveChanges();
}
