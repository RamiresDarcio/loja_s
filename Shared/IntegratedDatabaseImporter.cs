using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Felibow.Integration;

public static class IntegratedDatabaseImporter
{
    public static async Task ImportAvailableSourcesAsync(
        DbContext context,
        string storefrontDatabasePath,
        string inventoryDatabasePath,
        ILogger logger)
    {
        var connection = (SqliteConnection)context.Database.GetDbConnection();
        await context.Database.OpenConnectionAsync();
        try
        {
            await ExecuteAsync(connection, null, """
                CREATE TABLE IF NOT EXISTS "__FelibowImports" (
                    "Source" TEXT NOT NULL PRIMARY KEY,
                    "ImportedAt" TEXT NOT NULL
                );
                """);

            if (File.Exists(storefrontDatabasePath) &&
                await HasTableAsync(connection, null, "main", "Usuarios") &&
                await HasTableAsync(connection, null, "main", "Pedidos"))
            {
                await ImportSourceAsync(
                    connection,
                    storefrontDatabasePath,
                    "legacyShop",
                    "storefront-v1",
                    logger,
                    ImportStorefrontAsync);
            }

            if (File.Exists(inventoryDatabasePath) &&
                (await HasTableAsync(connection, null, "main", "Usuarios") ||
                 await HasTableAsync(connection, null, "main", "AdminUsuarios")) &&
                (await HasTableAsync(connection, null, "main", "Categorias") ||
                 await HasTableAsync(connection, null, "main", "AdminCategorias")))
            {
                await ImportSourceAsync(
                    connection,
                    inventoryDatabasePath,
                    "legacyInventory",
                    "inventory-v1",
                    logger,
                    ImportInventoryAsync);
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private static async Task ImportSourceAsync(
        SqliteConnection connection,
        string sourcePath,
        string alias,
        string sourceKey,
        ILogger logger,
        Func<SqliteConnection, SqliteTransaction, string, Task> import)
    {
        if (await HasImportedAsync(connection, sourceKey))
        {
            return;
        }

        await using (var attach = connection.CreateCommand())
        {
            attach.CommandText = $"ATTACH DATABASE $path AS {Quote(alias)};";
            attach.Parameters.AddWithValue("$path", sourcePath);
            await attach.ExecuteNonQueryAsync();
        }

        try
        {
            await using var transaction = connection.BeginTransaction();
            await import(connection, transaction, alias);
            await using var markImported = connection.CreateCommand();
            markImported.Transaction = transaction;
            markImported.CommandText = """
                INSERT INTO "__FelibowImports" ("Source", "ImportedAt")
                VALUES ($source, $importedAt);
                """;
            markImported.Parameters.AddWithValue("$source", sourceKey);
            markImported.Parameters.AddWithValue("$importedAt", DateTime.UtcNow.ToString("O"));
            await markImported.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            logger.LogInformation("Imported legacy data from {SourcePath} into the shared Felibow database.", sourcePath);
        }
        finally
        {
            await ExecuteAsync(connection, null, $"DETACH DATABASE {Quote(alias)};");
        }
    }

    private static async Task ImportStorefrontAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string alias)
    {
        await ImportStorefrontProductsAsync(connection, transaction, alias);

        var tables = new (string Source, string Target, bool RemapProductId)[]
        {
            ("Usuarios", "Usuarios", false),
            ("PerfisConta", "PerfisConta", false),
            ("SegurancasConta", "SegurancasConta", false),
            ("Enderecos", "Enderecos", false),
            ("Carrinhos", "Carrinhos", false),
            ("ItensCarrinho", "ItensCarrinho", true),
            ("Pedidos", "Pedidos", false),
            ("ItensPedido", "ItensPedido", true),
            ("Pagamentos", "Pagamentos", false),
            ("HistoricoPagamentos", "HistoricoPagamentos", false),
            ("MovimentacoesEstoque", "MovimentacoesEstoque", true),
            ("Favoritos", "Favoritos", true),
            ("MetodosPagamentoSalvos", "MetodosPagamentoSalvos", false),
            ("SessoesConta", "SessoesConta", false),
            ("TokensConta", "TokensConta", false),
            ("SolicitacoesSuporte", "SolicitacoesSuporte", false),
            ("Notificacoes", "Notificacoes", false)
        };

        await ImportTablesAsync(connection, transaction, alias, tables, "__LegacyShopProductMap");
    }

    private static async Task ImportStorefrontProductsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string alias)
    {
        if (!await HasTableAsync(connection, transaction, alias, "Produtos"))
        {
            return;
        }

        await ExecuteAsync(connection, transaction, """
            INSERT OR IGNORE INTO "Produtos"
                ("Id", "Nome", "SKU", "CodigoBarras", "CategoriaId", "FornecedorId",
                 "PrecoCusto", "PrecoVenda", "PrecoPromocional", "QuantidadeEstoque",
                 "EstoqueMinimo", "EstoqueMaximo", "Imagem", "Marca", "EdicaoEspecial",
                 "Descricao", "Status", "DataCadastro", "DataAtualizacao")
            SELECT s."Id", s."Nome",
                   CASE WHEN trim(s."SKU") = '' THEN 'LEGACY-SHOP-' || s."Id" ELSE s."SKU" END,
                   '', NULL, NULL, 0, s."Preco", NULL, s."Estoque", 0, 0,
                   COALESCE(s."ImagemUrl", ''), '', NULL, s."Descricao", s."Status",
                   s."DataCadastro", s."DataCadastro"
            FROM legacyShop."Produtos" AS s;
            """);

        await ExecuteAsync(connection, transaction, """
            INSERT OR IGNORE INTO "Produtos"
                ("Nome", "SKU", "CodigoBarras", "CategoriaId", "FornecedorId",
                 "PrecoCusto", "PrecoVenda", "PrecoPromocional", "QuantidadeEstoque",
                 "EstoqueMinimo", "EstoqueMaximo", "Imagem", "Marca", "EdicaoEspecial",
                 "Descricao", "Status", "DataCadastro", "DataAtualizacao")
            SELECT s."Nome",
                   CASE WHEN trim(s."SKU") = '' THEN 'LEGACY-SHOP-' || s."Id" ELSE s."SKU" END,
                   '', NULL, NULL, 0, s."Preco", NULL, s."Estoque", 0, 0,
                   COALESCE(s."ImagemUrl", ''), '', NULL, s."Descricao", s."Status",
                   s."DataCadastro", s."DataCadastro"
            FROM legacyShop."Produtos" AS s
            WHERE NOT EXISTS (
                SELECT 1 FROM "Produtos" AS p
                WHERE p."SKU" = CASE WHEN trim(s."SKU") = '' THEN 'LEGACY-SHOP-' || s."Id" ELSE s."SKU" END
            );
            """);

        await ExecuteAsync(connection, transaction, """
            CREATE TABLE IF NOT EXISTS "__LegacyShopProductMap" (
                "LegacyId" INTEGER NOT NULL PRIMARY KEY,
                "TargetId" INTEGER NOT NULL
            );
            """);
        await ExecuteAsync(connection, transaction, """
            INSERT OR REPLACE INTO "__LegacyShopProductMap" ("LegacyId", "TargetId")
            SELECT s."Id", p."Id"
            FROM legacyShop."Produtos" AS s
            JOIN "Produtos" AS p ON p."SKU" =
                CASE WHEN trim(s."SKU") = '' THEN 'LEGACY-SHOP-' || s."Id" ELSE s."SKU" END;
            """);
    }

    private static async Task ImportInventoryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string alias)
    {
        var tables = new (string Source, string Target, bool RemapProductId)[]
        {
            (await ResolveInventorySourceTableAsync(connection, transaction, alias, "Categorias", "AdminCategorias"), "AdminCategorias", false),
            (await ResolveInventorySourceTableAsync(connection, transaction, alias, "Fornecedores", "AdminFornecedores"), "AdminFornecedores", false),
            (await ResolveInventorySourceTableAsync(connection, transaction, alias, "Usuarios", "AdminUsuarios"), "AdminUsuarios", false),
            (await ResolveInventorySourceTableAsync(connection, transaction, alias, "Clientes", "AdminClientes"), "AdminClientes", false)
        };
        await ImportTablesAsync(connection, transaction, alias, tables, "__LegacyInventoryProductMap");

        await ImportInventoryProductsAsync(connection, transaction, alias);

        var transactionalTables = new (string Source, string Target, bool RemapProductId)[]
        {
            (await ResolveInventorySourceTableAsync(connection, transaction, alias, "MovimentacoesEstoque", "AdminMovimentacoesEstoque"), "AdminMovimentacoesEstoque", true),
            (await ResolveInventorySourceTableAsync(connection, transaction, alias, "Vendas", "AdminVendas"), "AdminVendas", false),
            (await ResolveInventorySourceTableAsync(connection, transaction, alias, "ItensVenda", "AdminItensVenda"), "AdminItensVenda", true)
        };
        await ImportTablesAsync(
            connection, transaction, alias, transactionalTables, "__LegacyInventoryProductMap");
    }

    private static async Task<string> ResolveInventorySourceTableAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string alias,
        string legacyName,
        string integratedName)
    {
        return await HasTableAsync(connection, transaction, alias, legacyName)
            ? legacyName
            : integratedName;
    }

    private static async Task ImportInventoryProductsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string alias)
    {
        if (!await HasTableAsync(connection, transaction, alias, "Produtos"))
        {
            return;
        }

        var sourceAvailableColumns = await GetColumnsAsync(connection, transaction, alias, "Produtos");
        string SourceValue(string column, string fallback) =>
            sourceAvailableColumns.Contains(column, StringComparer.OrdinalIgnoreCase)
                ? $"COALESCE(s.{Quote(column)}, {fallback})"
                : fallback;

        var dataCadastro = SourceValue("DataCadastro", "'2000-01-01T00:00:00.0000000'");
        var targetColumns = new[]
        {
            "Nome", "Descricao", "SKU", "CodigoBarras", "CategoriaId", "FornecedorId",
            "PrecoCusto", "PrecoVenda", "QuantidadeEstoque", "EstoqueMinimo", "EstoqueMaximo",
            "Imagem", "Marca", "Status", "DataCadastro", "DataAtualizacao"
        };
        var selectColumns = new[]
        {
            SourceValue("Nome", "''"),
            SourceValue("Descricao", "''"),
            SourceValue("SKU", "''"),
            SourceValue("CodigoBarras", "''"),
            SourceValue("CategoriaId", "NULL"),
            SourceValue("FornecedorId", "NULL"),
            SourceValue("PrecoCusto", "0"),
            SourceValue("PrecoVenda", "0"),
            SourceValue("QuantidadeEstoque", "0"),
            SourceValue("EstoqueMinimo", "0"),
            SourceValue("EstoqueMaximo", "0"),
            SourceValue("Imagem", "''"),
            SourceValue("Marca", "''"),
            SourceValue("Status", "'Ativo'"),
            dataCadastro,
            SourceValue("DataAtualizacao", dataCadastro)
        };
        var quotedTargetColumns = string.Join(", ", targetColumns.Select(Quote));
        var selectedValues = string.Join(", ", selectColumns);
        var legacyProducts = $"{Quote(alias)}.{Quote("Produtos")}";

        await ExecuteAsync(connection, transaction, $"""
            INSERT OR IGNORE INTO "Produtos"
                ("Id", {quotedTargetColumns}, "PrecoPromocional", "EdicaoEspecial")
            SELECT s."Id", {selectedValues}, NULL, NULL
            FROM {legacyProducts} AS s;
            """);
        await ExecuteAsync(connection, transaction, $"""
            INSERT OR IGNORE INTO "Produtos"
                ({quotedTargetColumns}, "PrecoPromocional", "EdicaoEspecial")
            SELECT {selectedValues}, NULL, NULL
            FROM {legacyProducts} AS s
            WHERE NOT EXISTS (
                SELECT 1 FROM "Produtos" AS p WHERE p."SKU" = {selectColumns[2]}
            );
            """);

        var updateColumns = targetColumns
            .Where(column => !column.Equals("SKU", StringComparison.OrdinalIgnoreCase))
            .Select(column => $"{Quote(column)} = {selectColumns[Array.IndexOf(targetColumns, column)]}");
        await ExecuteAsync(connection, transaction, $"""
            UPDATE "Produtos" AS p
            SET {string.Join(", ", updateColumns)}
            FROM {Quote(alias)}."Produtos" AS s
            WHERE p."SKU" = {selectColumns[2]};
            """);

        await ExecuteAsync(connection, transaction, """
            CREATE TABLE IF NOT EXISTS "__LegacyInventoryProductMap" (
                "LegacyId" INTEGER NOT NULL PRIMARY KEY,
                "TargetId" INTEGER NOT NULL
            );
            """);
        await ExecuteAsync(connection, transaction, $"""
            INSERT OR REPLACE INTO "__LegacyInventoryProductMap" ("LegacyId", "TargetId")
            SELECT s."Id", p."Id"
            FROM {Quote(alias)}."Produtos" AS s
            JOIN "Produtos" AS p ON p."SKU" = s."SKU";
            """);
    }

    private static async Task ImportTablesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string alias,
        IEnumerable<(string Source, string Target, bool RemapProductId)> tables,
        string productMapTable)
    {
        foreach (var (source, target, remapProductId) in tables)
        {
            if (!await HasTableAsync(connection, transaction, alias, source) ||
                !await HasTableAsync(connection, transaction, "main", target))
            {
                continue;
            }

            var sourceColumns = await GetColumnsAsync(connection, transaction, alias, source);
            var targetColumns = await GetColumnsAsync(connection, transaction, "main", target);
            var columns = targetColumns.Intersect(sourceColumns, StringComparer.OrdinalIgnoreCase).ToList();
            if (columns.Count == 0)
            {
                continue;
            }

            var sourceAlias = "legacyRow";
            var select = columns.Select(column =>
                remapProductId && column.Equals("ProdutoId", StringComparison.OrdinalIgnoreCase)
                    ? "productMap.\"TargetId\""
                    : $"{sourceAlias}.{Quote(column)}");
            var join = remapProductId
                ? $" JOIN {Quote(productMapTable)} AS productMap ON productMap.\"LegacyId\" = {sourceAlias}.\"ProdutoId\""
                : string.Empty;
            var where = remapProductId ? " WHERE productMap.\"TargetId\" IS NOT NULL" : string.Empty;
            await ExecuteAsync(connection, transaction, $"""
                INSERT OR IGNORE INTO {Quote(target)} ({string.Join(", ", columns.Select(Quote))})
                SELECT {string.Join(", ", select)}
                FROM {Quote(alias)}.{Quote(source)} AS {sourceAlias}{join}{where};
                """);
        }
    }

    private static async Task<List<string>> GetColumnsAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string schema,
        string table)
    {
        var columns = new List<string>();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"PRAGMA {Quote(schema)}.table_info({Quote(table)});";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task<bool> HasTableAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string schema,
        string table)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"SELECT 1 FROM {Quote(schema)}.sqlite_master WHERE type = 'table' AND name = $name LIMIT 1;";
        command.Parameters.AddWithValue("$name", table);
        return await command.ExecuteScalarAsync() != null;
    }

    private static async Task<bool> HasImportedAsync(SqliteConnection connection, string source)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT 1 FROM "__FelibowImports" WHERE "Source" = $source LIMIT 1;
            """;
        command.Parameters.AddWithValue("$source", source);
        return await command.ExecuteScalarAsync() != null;
    }

    private static async Task ExecuteAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static string Quote(string identifier) =>
        "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}
