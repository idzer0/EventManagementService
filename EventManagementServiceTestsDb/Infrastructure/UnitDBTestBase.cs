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
public abstract class UnitDBTestBase(PostgresFixture fixture)
{
    protected readonly PostgresFixture _fixture = fixture;

    protected AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        context.Database.Migrate();
        return context;
    }

    protected async Task ResetDatabaseAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Bookings\", \"Events\" RESTART IDENTITY CASCADE");
    }
}
