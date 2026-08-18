using Application.DTO;
using Domain.Models;
using AuthTestsDb.Infrastructure;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuthTestsDb;

[Collection("PostgresCollection")]
public class UserRepoTests(PostgresFixture fixture) : UnitDBTestBase(fixture)
{
    [Fact]
    public async Task CreateUser_ResultOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repo = new UserRepository(context, NullLogger<UserRepository>.Instance);
        var user = TestDataHelper.GetTestUser();

        // Act
        await repo.CreateUserAsync(user, CancellationToken.None);
        var result = await repo.GetUserByLoginAsync(user.Login, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Login.Should().Be(user.Login);
        result.Role.Should().Be(user.Role);
    }

}
