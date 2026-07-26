using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
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

    public AuthService(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
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
            ExpiresAt = DateTime.UtcNow.AddDays(7)
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
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };


        _context.RefreshTokens.Add(token);

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}