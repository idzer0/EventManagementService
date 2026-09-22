using System.Security.Cryptography;
using System.Text;
using EventMS.Auth.Application.Contracts;

namespace EventMS.Auth.Infrastructure.Services;

public class PasswordHasher(IUserRepository userRepository) : IPasswordHasher
{
    /// <inheritdoc/>
    public string GetHashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        return Convert.ToHexString(bytes);
    }

    /// <inheritdoc/>
    public async Task<bool> CheckPassword(string login, string password, CancellationToken ct)
    {
        var user = await userRepository.GetUserByLoginAsync(login, ct);

        return user is not null && user.PasswordHash == GetHashPassword(password);
    }

}
