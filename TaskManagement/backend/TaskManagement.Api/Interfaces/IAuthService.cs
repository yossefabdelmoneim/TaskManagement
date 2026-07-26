using TaskManagement.Api.DTOs.Auth;

namespace TaskManagement.Api.Interfaces;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    public Task<AuthResponseDto> LoginAsync(LoginDto dto);
}