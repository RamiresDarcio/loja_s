using Microsoft.Data.Sqlite;

namespace Felibow.Integration;

public static class SqliteConnectionStringResolver
{
    public static string Resolve(string connectionString, string contentRootPath)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (builder.DataSource == ":memory:" || Path.IsPathRooted(builder.DataSource))
        {
            return builder.ConnectionString;
        }

        if (Path.GetFileName(builder.DataSource).Equals(
            "felibow_integrated.db", StringComparison.OrdinalIgnoreCase))
        {
            var repositoryRoot = FindRepositoryRoot(contentRootPath);
            if (repositoryRoot is not null)
            {
                builder.DataSource = Path.Combine(repositoryRoot, "felibow_integrated.db");
                return builder.ConnectionString;
            }
        }

        builder.DataSource = Path.GetFullPath(builder.DataSource, contentRootPath);
        return builder.ConnectionString;
    }

    private static string? FindRepositoryRoot(string contentRootPath)
    {
        var current = new DirectoryInfo(Path.GetFullPath(contentRootPath));
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "loja_s")) &&
                Directory.Exists(Path.Combine(current.FullName, "bow.estoque")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}
