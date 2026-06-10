using EventManagementService.Contracts;
using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementServiceTestsDb.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTestsDb;

[Collection("Операции c таблицей Bookings")]
public class BookingRepoTests : UnitDBTestBase
{
    [Fact]
    public async Task CheckInsertConstraint_ReturnThrow()
    {
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
        Func<Task> act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync()
            .WithInnerException<Exception>(typeof(Exception), "")
            .WithMessage("23503: insert or update on table \"Bookings\" violates foreign key constraint \"FK_Bookings_Events_EventId\"*");
    }

    [Fact]
    public async Task Delete_CheckConstraint_ReturnThrow()
    {
        var evt = new EventEntity()
        {
            Id = Guid.NewGuid(),
            Title = "Событие для проверки ограничения на удаление",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        };

        var booking = new BookingEntity()
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            Status = BookingStatusEnum.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Events.AddAsync(evt);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        Func<Task> act = async () =>
        {
            context.Events.Remove(evt);
            await context.SaveChangesAsync();
        };

        await act.Should().ThrowAsync()
            .WithMessage("The association between entity types 'EventEntity' and 'BookingEntity' has been severed, but the relationship is either marked as required or is implicitly required because the foreign key is not nullable*");
    }

    [Fact]
    public async Task Update_CheckConstraint_ReturnThrow()
    {
        var evt = new EventEntity()
        {
            Id = Guid.NewGuid(),
            Title = "Событие для проверки ограничения на удаление",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        };

        var booking = new BookingEntity()
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            Status = BookingStatusEnum.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Events.AddAsync(evt);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        booking.EventId = Guid.NewGuid();

        Func<Task> act = async () =>
        {
            context.Bookings.Update(booking);
            await context.SaveChangesAsync();
        };

        await act.Should().ThrowAsync()
            .WithInnerException<Exception>(typeof(Exception), "")
            .WithMessage("23503: insert or update on table \"Bookings\" violates foreign key constraint \"FK_Bookings_Events_EventId\"*");
    }


    [Fact]
    public async Task GetBookingIdsByStatusAsync_CheckOrderByStartAt_ResultOk()
    {
        var evt = new EventEntity()
        {
            Id = Guid.NewGuid(),
            Title = "Событие для проверки ограничения на удаление",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(10),
            TotalSeats = 100,
            AvailableSeats = 100,
        };

        var bookings = new List<BookingEntity>();
        for (int i = 0; i < 20; i++)
        {
            bookings.Add(new BookingEntity()
            {
                Id = Guid.NewGuid(),
                EventId = evt.Id,
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-i),
            });
        }
        Guid testGuid = bookings[19].Id;

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Events.AddAsync(evt);
        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();

        var repo = new BookingRepository(context, NullLogger<BookingRepository>.Instance);

        var orderedResult = await repo.GetBookingIdsByStatusAsync(BookingStatusEnum.Pending, CancellationToken.None);

        orderedResult[0].Should().Be(testGuid);
        orderedResult.Count.Should().Be(10);
    }

}