using System.Security.Cryptography;
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
