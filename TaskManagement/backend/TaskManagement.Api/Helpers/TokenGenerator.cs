using System.Security.Cryptography;

namespace TaskManagement.Api.Helpers;

public static class TokenGenerator
{
    public static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }
}