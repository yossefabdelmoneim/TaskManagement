using TaskManagement.Api.DTOs.Auth;

namespace TaskManagement.Api.Interfaces;

public interface IAuthService
{
    public Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    public Task<AuthResponseDto> LoginAsync(LoginDto dto);

    public Task LogoutAsync(LogoutRequestDto dto);

     public Task LogoutAllAsync(int userId);

    public Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
}