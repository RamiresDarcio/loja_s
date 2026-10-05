using Microsoft.EntityFrameworkCore;

namespace loja_s.Data;

public static class ContaSchema
{
    public static async Task EnsureCreatedAsync(ApplicationDbContext db)
    {
        var statements = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS "PerfisConta" (
                "UsuarioId" INTEGER NOT NULL PRIMARY KEY REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                "Sobrenome" TEXT NOT NULL,
                "DataNascimento" TEXT NULL,
                "TermosAceitosEm" TEXT NULL,
                "EmailConfirmado" INTEGER NOT NULL DEFAULT 0,
                "AtualizacoesPedidos" INTEGER NOT NULL DEFAULT 1,
                "Promocoes" INTEGER NOT NULL DEFAULT 0,
                "Novidades" INTEGER NOT NULL DEFAULT 0,
                "ProdutosFavoritos" INTEGER NOT NULL DEFAULT 1,
                "AlertasSeguranca" INTEGER NOT NULL DEFAULT 1
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "SegurancasConta" (
                "UsuarioId" INTEGER NOT NULL PRIMARY KEY REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                "TentativasFalhas" INTEGER NOT NULL DEFAULT 0,
                "BloqueadoAte" TEXT NULL,
                "SecurityStamp" TEXT NOT NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "Favoritos" (
                "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "UsuarioId" INTEGER NOT NULL REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                "ProdutoId" INTEGER NOT NULL REFERENCES "Produtos" ("Id") ON DELETE CASCADE,
                "DataAdicionado" TEXT NOT NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "MetodosPagamentoSalvos" (
                "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "UsuarioId" INTEGER NOT NULL REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                "Bandeira" TEXT NOT NULL,
                "UltimosQuatro" TEXT NOT NULL,
                "TokenProvedor" TEXT NOT NULL,
                "Preferencial" INTEGER NOT NULL DEFAULT 0,
                "DataCadastro" TEXT NOT NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "SessoesConta" (
                "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "UsuarioId" INTEGER NOT NULL REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                "ChaveSessao" TEXT NOT NULL,
                "Dispositivo" TEXT NOT NULL,
                "CriadaEm" TEXT NOT NULL,
                "UltimaAtividade" TEXT NOT NULL
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "TokensConta" (
                "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "UsuarioId" INTEGER NOT NULL REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                "Finalidade" TEXT NOT NULL,
                "TokenHash" TEXT NOT NULL,
                "ExpiraEm" TEXT NOT NULL,
                "Utilizado" INTEGER NOT NULL DEFAULT 0
            );
            """,
            """
            CREATE TABLE IF NOT EXISTS "SolicitacoesSuporte" (
                "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                "UsuarioId" INTEGER NOT NULL REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
                "Categoria" TEXT NOT NULL,
                "Assunto" TEXT NOT NULL,
                "Mensagem" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "DataCriacao" TEXT NOT NULL
            );
            """
        };

        foreach (var statement in statements)
        {
            await db.Database.ExecuteSqlRawAsync(statement);
        }

        await CreateIndexIfMissingAsync(db,
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Favoritos_UsuarioId_ProdutoId\" ON \"Favoritos\" (\"UsuarioId\", \"ProdutoId\");");
        await CreateIndexIfMissingAsync(db,
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_SessoesConta_ChaveSessao\" ON \"SessoesConta\" (\"ChaveSessao\");");
        await CreateIndexIfMissingAsync(db,
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_TokensConta_TokenHash_Finalidade\" ON \"TokensConta\" (\"TokenHash\", \"Finalidade\");");
        await CreateIndexIfMissingAsync(db,
            "CREATE INDEX IF NOT EXISTS \"IX_MetodosPagamentoSalvos_UsuarioId\" ON \"MetodosPagamentoSalvos\" (\"UsuarioId\");");

        await AddColumnIfMissingAsync(db, "Enderecos", "Pais",
            "ALTER TABLE \"Enderecos\" ADD COLUMN \"Pais\" TEXT NOT NULL DEFAULT 'Brasil';");
        await AddColumnIfMissingAsync(db, "Enderecos", "Principal",
            "ALTER TABLE \"Enderecos\" ADD COLUMN \"Principal\" INTEGER NOT NULL DEFAULT 0;");
    }

    private static async Task CreateIndexIfMissingAsync(ApplicationDbContext db, string statement)
    {
        await db.Database.ExecuteSqlRawAsync(statement);
    }

    private static async Task AddColumnIfMissingAsync(ApplicationDbContext db, string table, string column, string statement)
    {
        var columns = await db.Database.SqlQuery<string>(
            $"SELECT name AS \"Value\" FROM pragma_table_info({table})").ToListAsync();
        if (!columns.Contains(column, StringComparer.OrdinalIgnoreCase))
        {
            await db.Database.ExecuteSqlRawAsync(statement);
        }
    }
}
