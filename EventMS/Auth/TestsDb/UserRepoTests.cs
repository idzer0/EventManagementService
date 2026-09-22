using EventMS.Auth.Application.DTO;
using EventMS.Auth.Domain.Models;
using EventMS.Auth.AuthTestsDb.Infrastructure;
using FluentAssertions;
using EventMS.Auth.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using static EventMS.Auth.AuthTestsDb.Infrastructure.TestDataHelper;

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
