using Application.Contracts;
using Application.Services;
using Auth.Contracts;
using Bookings.Tests.Infrastructure;
using Bookings.TestsDb.Infrastructure;
using Domain.DomainExceptions;
using Domain.Models;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static Bookings.TestsDb.Infrastructure.TestDataHelper;

namespace Bookings.TestsDb;

[Collection("PostgresCollection")]
public class BookingsTableTests(PostgresFixture fixture) : UnitDBTestBase(fixture)
{

    [Fact]
    public async Task GetBookingIdsByStatusAsync_CheckOrderByStartAt_ResultOk()
    {
        // Arrange
        var currentUserService = TestDataHelper.GetCurrentUserService(null, null);

        Guid eventId = Guid.NewGuid();

        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var bookings = new List<BookingEntity>();
        for (int i = 0; i < 20; i++)
        {
            bookings.Add(new BookingEntity()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-i),
                UserId = 1,
            });
        }
        Guid testGuid = bookings[19].Id;

        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();

        var repo = new BookingRepository(context, currentUserService, NullLogger<BookingRepository>.Instance);

        // Act
        var orderedResult = await repo.GetBookingIdsByStatusAsync(BookingStatusEnum.Pending, CancellationToken.None);

        // Assert
        orderedResult[0].Should().Be(testGuid);
        orderedResult.Count.Should().Be(10);
    }

    [Fact]
    public async Task CreateAndUpdateAsync_ReturnUpdated()
    {
        Guid eventId = Guid.NewGuid();

        var currentUserService = TestDataHelper.GetCurrentUserService(1, UsersRole.User);

        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repo = new BookingRepository(context, currentUserService, NullLogger<BookingRepository>.Instance);

        // Act
        var book = await repo.CreateBookingAsync(
            eventId,
            BookingStatusEnum.Pending,
            DateTimeOffset.UtcNow.AddDays(-1),
            CancellationToken.None);

        book.Status = BookingStatusEnum.Rejected;
        await repo.UpdateBookingAsync(book, CancellationToken.None);

        var result = await repo.GetBookingByIdAsync(book.Id, CancellationToken.None);

        // Assert
        result?.Status.Should().Be(BookingStatusEnum.Rejected);
    }

    [Fact]
    public async Task CreateBookingAsync_CancelOtherBooking_NoAuth()
    {
        // Arrange
        var currentUserService1 = TestDataHelper.GetCurrentUserService(1, UsersRole.User);
        var currentUserService2 = TestDataHelper.GetCurrentUserService(2, UsersRole.User);

        var eventId = Guid.NewGuid();
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repoBooking1 = new BookingRepository(context, currentUserService1, NullLogger<BookingRepository>.Instance);
        var repoBooking2 = new BookingRepository(context, currentUserService2, NullLogger<BookingRepository>.Instance);

        var srv1 = ServiceMocker.ArrangeBookingServiceTestCase(context, currentUserService1, null);
        var srv2 = ServiceMocker.ArrangeBookingServiceTestCase(context, currentUserService2, null);


        var book = await srv1.CreateBookingAsync(eventId, CancellationToken.None);

        // Act
        Func<Task> act = async () => await srv2.CancelAsync(book.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessDomainException>()
            .WithMessage("Недостаточно прав");
    }
}
