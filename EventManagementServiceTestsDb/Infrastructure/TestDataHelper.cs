using Application.Contracts;
using Domain.Models;
using Moq;

namespace EventManagementServiceTestsDb.Infrastructure;

public static class TestDataHelper
{
    public static EventEntity GetEventEntity(
        Guid? eventId = null,
        string title = "Test event",
        string description = "Test event",
        DateTime? startAt = null,
        DateTime? endAt = null,
        int totalSeats = 5,
        int availableSeats = 5)
    {
        return new EventEntity
        {
            Id = eventId ?? Guid.NewGuid(),
            Title = title,
            Description = description,
            StartAt = (startAt ?? DateTime.UtcNow).Date.AddDays(1),
            EndAt = (endAt ?? DateTime.UtcNow).Date.AddDays(2),
            TotalSeats = totalSeats,
            AvailableSeats = availableSeats,
        };
    }

    public static List<EventEntity> GetListEventEntity(bool ordered, int count, DateTime? startAt = null, int numSeats = 5)
    {
        DateTime date = (startAt ?? DateTime.UtcNow);

        var events = new List<EventEntity>();
        for (int i = 1; i <= count; i++)
        {
            events.Add(new EventEntity
            {
                Id = Guid.NewGuid(),
                Title = $"Событие {i}",
                StartAt = i == count && !ordered? date.AddDays(-1) : date.AddDays(i),
                EndAt = date.AddDays(i + 1),
                TotalSeats = numSeats,
                AvailableSeats = numSeats,
            });
        }

        return events;
    }

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
