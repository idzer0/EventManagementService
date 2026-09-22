using EventMS.Bookings.Application.Contracts;
using EventMS.Auth.Contracts;
using EventMS.Bookings.Domain.Models;
using Moq;

namespace EventMS.Bookings.TestsDb.Infrastructure;

public static class TestDataHelper
{
    public static ICurrentUserService GetCurrentUserService(int? userId, UsersRole? role)
    {
        Mock<ICurrentUserService> cus = new();
        cus.Setup(c => c.UserId).Returns(userId);
        cus.Setup(c => c.Role).Returns(role);
        cus.Setup(c => c.IsAllowUserOperation(userId)).Returns(true);

        if (role == UsersRole.Admin)
        {
            cus.Setup(c => c.IsAllowAdminOperation()).Returns(true);
        }

        return cus.Object;
    }

}
