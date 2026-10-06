using System.Security.Cryptography;
using System.Text;
using bow.estoque.Models;
using Microsoft.AspNetCore.Identity;

namespace bow.estoque.Services;

public static class PasswordHashing
{
    private static readonly PasswordHasher<Usuario> Hasher = new();

    public static string Hash(string password)
    {
        return Hasher.HashPassword(new Usuario(), password);
    }

    public static bool Verify(string password, string hash, out string? upgradedHash)
    {
        upgradedHash = null;

        if (hash.Length == 64 && hash.All(Uri.IsHexDigit))
        {
            var expectedHash = Convert.FromHexString(hash);
            var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var isValid = CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            if (isValid)
            {
                upgradedHash = Hash(password);
            }

            return isValid;
        }

        var result = Hasher.VerifyHashedPassword(new Usuario(), hash, password);
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            upgradedHash = Hash(password);
        }

        return result != PasswordVerificationResult.Failed;
    }
}
