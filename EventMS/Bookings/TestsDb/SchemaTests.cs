using EventMS.Bookings.TestsDb.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventMS.Bookings.TestsDb;

[Collection("PostgresCollection")]
public class SchemaTests (PostgresFixture fixture) : UnitDBTestBase(fixture)
{
[Fact]
    public async Task Migrate_CreatesEventsTable()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        await using var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var commandBookingsTable = connection.CreateCommand();

        // Act
        commandBookingsTable.CommandText = "SELECT to_regclass('public.\"bookings\"') IS NOT NULL";

        // Assert
        Assert.True((bool)(await commandBookingsTable.ExecuteScalarAsync())!);
    }
}
