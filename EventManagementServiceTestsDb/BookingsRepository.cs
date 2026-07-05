using Domain.Models;
using EventManagementServiceTestsDb.Infrastructure;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTestsDb;

[Collection("PostgresCollection")]
public class BookingsTableTests(PostgresFixture fixture) : UnitDBTestBase(fixture)
{

    [Fact]
    public async Task GetBookingIdsByStatusAsync_CheckOrderByStartAt_ResultOk()
    {
        // Arrange
        var evt = TestDataHelper.GetEventEntity(totalSeats: 100, availableSeats: 100);

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

        // Act
        var orderedResult = await repo.GetBookingIdsByStatusAsync(BookingStatusEnum.Pending, CancellationToken.None);

        // Assert
        orderedResult[0].Should().Be(testGuid);
        orderedResult.Count.Should().Be(10);
    }

    [Fact]
    public async Task CreateAndUpdateAsync_ReturnUpdated()
    {
        var evt = TestDataHelper.GetEventEntity(totalSeats: 100, availableSeats: 100);

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Events.AddAsync(evt);
        await context.SaveChangesAsync();

        var repo = new BookingRepository(context, NullLogger<BookingRepository>.Instance);

        // Act
        var book = await repo.CreateBookingAsync(
            evt.Id,
            BookingStatusEnum.Pending,
            DateTimeOffset.UtcNow.AddDays(-1),
            CancellationToken.None);

        book.Status = BookingStatusEnum.Rejected;
        await repo.UpdateBookingAsync(book, CancellationToken.None);

        var result = await repo.GetBookingByIdAsync(book.Id, CancellationToken.None);

        // Assert
        result?.Status.Should().Be(BookingStatusEnum.Rejected);
    }

}
