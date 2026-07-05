using EventManagementServiceTestsDb.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTestsDb;

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

        await using var commandEventsTable = connection.CreateCommand();
        await using var commandBoonikgsTable = connection.CreateCommand();
        await using var commandTrgmExtExist = connection.CreateCommand();
        await using var commandTrgmIndexExist = connection.CreateCommand();

        // Act
        commandBoonikgsTable.CommandText = "SELECT to_regclass('public.\"Bookings\"') IS NOT NULL";
        commandEventsTable.CommandText = "SELECT to_regclass('public.\"Events\"') IS NOT NULL";
        commandTrgmExtExist.CommandText = "SELECT true FROM pg_extension WHERE extname = 'pg_trgm';";
        commandTrgmIndexExist.CommandText = @"
        select true 
        FROM pg_index x 
            JOIN pg_class i ON i.oid = x.indexrelid 
            JOIN pg_class c ON c.oid = x.indrelid 
            JOIN pg_namespace n ON n.oid = c.relnamespace 
            JOIN pg_am am ON am.oid = i.relam
            JOIN pg_opclass opc ON opc.oid = x.indclass[0]
            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = ANY(x.indkey)
        WHERE
            n.nspname = 'public'
            AND c.relname = 'Events'
            AND a.attname = 'Title'
            AND opc.opcname IN ('gist_trgm_ops', 'gin_trgm_ops');";

        // Assert
        Assert.True((bool)(await commandBoonikgsTable.ExecuteScalarAsync())!);
        Assert.True((bool)(await commandEventsTable.ExecuteScalarAsync())!);
        Assert.True((bool)(await commandTrgmExtExist.ExecuteScalarAsync())!);
        Assert.True((bool)(await commandTrgmIndexExist.ExecuteScalarAsync())!);
    }
}
