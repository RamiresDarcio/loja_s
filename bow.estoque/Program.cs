using bow.estoque.Data;
using bow.estoque.Models;
using bow.estoque.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=bow_estoque.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(Felibow.Integration.SqliteConnectionStringResolver.Resolve(
            connectionString, builder.Environment.ContentRootPath));
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

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
    await EnsureAdminSchemaAsync(db);
    await EnsureProductsTableAsync(db);
    await Felibow.Integration.IntegratedDatabaseImporter.ImportAvailableSourcesAsync(
        db,
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "loja_s", "loja_s.db")),
        Path.Combine(builder.Environment.ContentRootPath, "bow_estoque.db"),
        app.Logger);
    SeedAdminUser(db, app.Configuration, app.Environment, app.Logger);
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
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

static async Task EnsureAdminSchemaAsync(ApplicationDbContext context)
{
    var createScript = context.Database.GenerateCreateScript()
        .Replace("CREATE UNIQUE INDEX ", "CREATE UNIQUE INDEX IF NOT EXISTS ", StringComparison.OrdinalIgnoreCase)
        .Replace("CREATE INDEX ", "CREATE INDEX IF NOT EXISTS ", StringComparison.OrdinalIgnoreCase)
        .Replace("CREATE TABLE ", "CREATE TABLE IF NOT EXISTS ", StringComparison.OrdinalIgnoreCase);
    await context.Database.ExecuteSqlRawAsync(createScript);
}

static async Task EnsureProductsTableAsync(ApplicationDbContext context)
{
    if (!context.Database.IsSqlite())
    {
        return;
    }

    var productsTableExists = await context.Database.SqlQueryRaw<int>(
        """SELECT COUNT(*) AS "Value" FROM sqlite_master WHERE type = 'table' AND name = 'Produtos'""")
        .SingleAsync();
    if (productsTableExists == 0)
    {
        throw new InvalidOperationException(
            $"The SQLite database '{context.Database.GetDbConnection().DataSource}' is missing the required Produtos table after schema initialization.");
    }
}

static void SeedAdminUser(
    ApplicationDbContext context,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger logger)
{
    if (!environment.IsDevelopment() || context.Usuarios.Any())
    {
        return;
    }

    var username = configuration["DevelopmentAdmin:Username"];
    var password = configuration["DevelopmentAdmin:Password"];
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    {
        logger.LogWarning(
            "No development administrator was created. Configure DevelopmentAdmin:Username and DevelopmentAdmin:Password using user secrets or environment variables.");
        return;
    }

    var usuario = new Usuario
    {
        NomeUsuario = username,
        NomeCompleto = "Administrador do Sistema",
        Email = $"{username}@bowestoque.local",
        SenhaHash = PasswordHashing.Hash(password),
        Status = "Ativo",
        DataCadastro = DateTime.Now
    };

    context.Usuarios.Add(usuario);
    context.SaveChanges();
}
