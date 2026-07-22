using Domain.Models;
using EventManagementServiceTestsDb.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTestsDb;

[Collection("PostgresCollection")]
public class BookingCheckConstraintsTests(PostgresFixture fixture) : UnitDBTestBase(fixture)
{
    [Fact]
    public async Task AddAsync_CheckConstraint_ReturnThrow()
    {
        // Arrange
        var booking = new BookingEntity()
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            Status = BookingStatusEnum.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Bookings.AddAsync(booking);

        // Act
        Func<Task> act = async () => await context.SaveChangesAsync();

        //Assert
        await act.Should().ThrowAsync()
            .WithInnerException<Exception>(typeof(Exception), "")
            .WithMessage("23503: insert or update on table \"Bookings\" violates foreign key constraint \"FK_Bookings_Events_EventId\"*");
    }


    [Fact]
    public async Task Delete_CheckConstraint_ReturnThrow()
    {
        // Arrange
        var evt = new EventEntity()
        {
            Id = Guid.NewGuid(),
            Title = "Событие для проверки ограничения на удаление",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        };

        await ResetDatabaseAsync();
        await using var context = CreateContext();

        await context.Users.AddAsync(TestDataHelper.GetTestUser());
        await context.SaveChangesAsync();

        var booking = new BookingEntity()
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            Status = BookingStatusEnum.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = 1,
        };

        await context.Events.AddAsync(evt);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
        {
            context.Events.Remove(evt);
            await context.SaveChangesAsync();
        };

        //Assert
        await act.Should().ThrowAsync()
            .WithMessage("The association between entity types 'EventEntity' and 'BookingEntity' has been severed, but the relationship is either marked as required or is implicitly required because the foreign key is not nullable*");
    }

    [Fact]
    public async Task Update_CheckConstraint_ReturnThrow()
    {
        // Arrange
        var evt = new EventEntity()
        {
            Id = Guid.NewGuid(),
            Title = "Событие для проверки ограничения на удаление",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        };

        await ResetDatabaseAsync();
        await using var context = CreateContext();

        await context.Users.AddAsync(TestDataHelper.GetTestUser());
        await context.SaveChangesAsync();


        var booking = new BookingEntity()
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            Status = BookingStatusEnum.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = 1,
        };

        await context.Events.AddAsync(evt);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        booking.EventId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
        {
            context.Bookings.Update(booking);
            await context.SaveChangesAsync();
        };

        //Assert
        await act.Should().ThrowAsync()
            .WithInnerException<Exception>(typeof(Exception), "")
            .WithMessage("23503: insert or update on table \"Bookings\" violates foreign key constraint \"FK_Bookings_Events_EventId\"*");
    }


}