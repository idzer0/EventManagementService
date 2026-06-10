using EventManagementService.Contracts;
using EventManagementService.Infrastructure.DataAccess;
using EventManagementService.Models;
using EventManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventManagementServiceTestsDb.Infrastructure;

/// <summary>
/// Базовый класс Unit тестов для проверки ограничений БД
/// </summary>
public abstract class UnitDBTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgcontainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("eventapi-test")
        .WithTmpfsMount("/var/lib/postgresql/data") // Размещаем БД в памяти
        .Build();

    public async Task InitializeAsync()
    {
        await _pgcontainer.StartAsync();
        await _pgcontainer.ExecScriptAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
    }

    public async Task DisposeAsync()
    {
        await _pgcontainer.DisposeAsync();
    }

    /// <summary>
    /// Инициализация контекста БД
    /// </summary>
    protected AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_pgcontainer.GetConnectionString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Сброс тестовых данных
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
             "TRUNCATE TABLE \"Bookings\", \"Events\" RESTART IDENTITY CASCADE");
    }
}
