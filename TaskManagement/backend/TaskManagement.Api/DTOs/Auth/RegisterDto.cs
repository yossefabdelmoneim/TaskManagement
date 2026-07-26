using TaskManagement.Api.Enums;

namespace TaskManagement.Api.DTOs.Auth;

public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = UserRole.User.ToString();
}