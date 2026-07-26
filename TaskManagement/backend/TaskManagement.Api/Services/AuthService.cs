using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManagement.Api.Configurations;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs.Auth;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;
using TaskManagement.Api.Exceptions;
using TaskManagement.Api.Helpers;

namespace TaskManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, ITokenService tokenService, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var ExistingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);

        if (ExistingUser)
        {
            throw new UnauthorizedException("User with this email already exists.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var accessToken = _tokenService.GenerateToken(user);

        var refreshToken = TokenGenerator.GenerateRefreshToken();

        var token = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };

        _context.RefreshTokens.Add(token);

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var accessToken = _tokenService.GenerateToken(user);

        var refreshToken = TokenGenerator.GenerateRefreshToken();

         var token = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };


        _context.RefreshTokens.Add(token);

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task LogoutAsync(LogoutRequestDto dto)
    {
        var refreshToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (refreshToken is null)
            throw new UnauthorizedException("Invalid refresh token.");

        if (refreshToken.IsRevoked)
            throw new UnauthorizedException("Refresh token already revoked.");

        refreshToken.IsRevoked = true;

        await _context.SaveChangesAsync();
    }

    public async Task LogoutAllAsync(int userId)
    {
        var refreshTokens = await _context.RefreshTokens
        .Where(rt => rt.UserId == userId && !rt.IsRevoked)
        .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }
   public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
{
    var storedToken = await _context.RefreshTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

    if (storedToken is null)
        throw new UnauthorizedException("Refresh token is invalid.");

    if (storedToken.IsRevoked)
        throw new UnauthorizedException("Refresh token has been revoked.");

    if (storedToken.ExpiresAt <= DateTime.UtcNow)
        throw new UnauthorizedException("Refresh token has expired.");

    // Revoke the old token
    storedToken.IsRevoked = true;
    // Later we'll add:
    // storedToken.RevokedAt = DateTime.UtcNow;

    var accessToken = _tokenService.GenerateToken(storedToken.User);
    var refreshToken = TokenGenerator.GenerateRefreshToken();

    var newRefreshToken = new RefreshToken
    {
        Token = refreshToken,
        UserId = storedToken.UserId,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
        IsRevoked = false
    };

    _context.RefreshTokens.Add(newRefreshToken);

    await _context.SaveChangesAsync();

    return new AuthResponseDto
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken
    };
}
}
