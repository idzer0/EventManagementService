using System.Security.Cryptography;
using System.Text;
using Application.Contracts;

namespace Infrastructure.Services;

public class PasswordHasher(IUserRepository userRepository) : IPasswordHasher
{
    public string GetHashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        return Convert.ToHexString(bytes);
    }

    public async Task<bool> CheckPassword(string login, string password, CancellationToken ct)
    {
        var user = await userRepository.GetUserByLoginAsync(login, ct);

        return user is not null && user.PasswordHash == GetHashPassword(password);
    }

}
