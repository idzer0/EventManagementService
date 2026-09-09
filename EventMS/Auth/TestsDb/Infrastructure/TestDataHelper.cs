using EventMS.Auth.Application.Contracts;
using EventMS.Auth.Contracts;
using EventMS.Auth.Domain.Models;
using Moq;

namespace EventMS.Auth.AuthTestsDb.Infrastructure;

public static class TestDataHelper
{
    public static UserEntity GetTestUser()
    {
        return new UserEntity()
        {
            Id = 1,
            Login = "user",
            PasswordHash = "1",
            Role = UsersRole.User
        };
    }

    public static UserEntity GetTestAdmin()
    {
        return new UserEntity()
        {
            Id = 2,
            Login = "admin",
            PasswordHash = "2",
            Role = UsersRole.Admin
        };
    }
}
