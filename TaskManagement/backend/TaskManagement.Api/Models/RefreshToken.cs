namespace TaskManagement.Api.Models;

public class RefreshToken
{
    public int Id { get; set; }

    // The actual random token.
    public string Token { get; set; } = string.Empty;

    // When it expires.
    public DateTime ExpiresAt { get; set; }

    // When it was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Was it manually revoked?
    public bool IsRevoked { get; set; }

    // FK to User.
    public int UserId { get; set; }

    // Navigation property.
    public User User { get; set; } = null!;
}