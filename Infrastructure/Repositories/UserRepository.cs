using Application.Contracts;
using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository (AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    ///<inheritdoc>
    public Task<bool> IsExistsAsync(string login, CancellationToken ct)
    {
        return _context.Users.AnyAsync(u => u.Login == login.ToLower(), ct);
    }


    ///<inheritdoc>
    public async Task CreateUserAsync(UserEntity user, CancellationToken ct)
    {
        try
        {
            await _context.AddAsync(user, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания пользователя");
            throw;
        }
    }

    ///<inheritdoc>
    public Task<UserEntity?> GetUserByLoginAsync(string login, CancellationToken ct)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Login == login.ToLower(), ct);
    }
}
