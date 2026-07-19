using System.Security.Authentication;
using System.Security.Claims;
using Application.Contracts;
using Application.DTO;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using static System.Security.Claims.ClaimTypes;

namespace Infrastructure.Services;

public class AuthService(
    IJwtTokenGenerator jwtTokenGenerator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IAuthService
{

    private string GenerateToken(string login, UsersRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Role, role.ToString()),
        };

        return jwtTokenGenerator.GenerateToken(claims, DateTime.UtcNow.AddHours(1));
    }

    /// <inheritdoc/>
    public async Task RegisterAsync(string login, string password, UsersRole role, CancellationToken ct)
    {
        UserEntity newUser = new()
        {
            Login = login.ToLower(),
            PasswordHash = passwordHasher.GetHashPassword(password),
            Role = role,
        };

        await userRepository.CreateUserAsync(newUser, ct);
    }

    /// <inheritdoc/>
    public async Task<string> LoginAsync(string login, string password, CancellationToken ct)
    {
        var user = await userRepository.GetUserByLoginAsync(login, ct);

        if (user is null || user.PasswordHash != passwordHasher.GetHashPassword(password))
            throw new AuthenticationException("Неверный логин или пароль");

        return GenerateToken(login, user.Role);
    }
}
