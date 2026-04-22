using System.Security.Cryptography;
using System.Text;

namespace PokeBuilder.Server.Security;

public static class RefreshTokenHasher
{
    public static string Hash(string plaintext)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToBase64String(bytes);
    }
}
