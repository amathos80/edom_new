using System.Security.Cryptography;
using System.Text;

namespace eDom.Application.Common.Auth;

public static class AuthHelpers
{
    public static string HashPassword(this string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA512.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}