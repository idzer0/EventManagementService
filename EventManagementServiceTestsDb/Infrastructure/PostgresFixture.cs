using Npgsql;
using Testcontainers.PostgreSql;

namespace EventManagementServiceTestsDb.Infrastructure;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgcontainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("eventapi-test")
        .WithTmpfsMount("/var/lib/postgresql/data")
        .Build();

    public string ConnectionString => _pgcontainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _pgcontainer.StartAsync();
        await _pgcontainer.ExecScriptAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
    }

    public async Task DisposeAsync()
    {
        await _pgcontainer.DisposeAsync();
    }

    // Метод для сброса данных
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        var command = new NpgsqlCommand("TRUNCATE TABLE \"Bookings\", \"Events\" RESTART IDENTITY CASCADE", connection);
        await command.ExecuteNonQueryAsync();
    }
}
