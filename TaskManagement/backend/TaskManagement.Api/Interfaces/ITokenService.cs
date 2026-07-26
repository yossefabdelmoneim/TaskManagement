using TaskManagement.Api.Models;

namespace TaskManagement.Api.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}