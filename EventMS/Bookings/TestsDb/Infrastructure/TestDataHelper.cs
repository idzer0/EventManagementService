using Application.Contracts;
using Auth.Contracts;
using Domain.Models;
using Moq;

namespace Bookings.TestsDb.Infrastructure;

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
