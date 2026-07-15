namespace Application.Contracts;

public interface IPasswordHasher
{
    string GetHashPassword(string password);

    Task<bool> CheckPassword(string login, string password, CancellationToken ct);
}
