using System.Security.Authentication;
using System.Security.Claims;
using EventMS.Auth.Application.Contracts;
using EventMS.Auth.Application.DTO;
using EventMS.Auth.Domain.Models;
using EventMS.Auth.Contracts;
using Microsoft.AspNetCore.Identity;
using static System.Security.Claims.ClaimTypes;

namespace EventMS.Auth.Infrastructure.Services;

public class AuthService(
    IJwtTokenGenerator jwtTokenGenerator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IAuthService
{

    private string GenerateToken(int userId, string login, UsersRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
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

        return GenerateToken(user.Id, login, user.Role);
    }
}
