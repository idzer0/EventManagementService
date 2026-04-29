using EventManagementService.Contracts;
using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementService.ServicesBackground;
using EventManagementServiceTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTests;

[Collection("Успешные сценарии сервиса бронирования")]
public class BookingServiceTestsPositive
{
    private readonly DbContextMocker _dbContextMocker;

    public BookingServiceTestsPositive ()
    {
        _dbContextMocker = new DbContextMocker();
    }

    [Fact]
    public async Task CreateAsync_ValidOneBooking_ReturnsPendingBooking()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateAsync_ValidOneBooking_ReturnsPendingBooking));
        var eventService = _dbContextMocker.ArrangeEventServiceTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, eventService, null);

        var result = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatusEnum.Pending);
    }

    [Fact]
    public async Task CreateAsync_ValidSomeBooking_ReturnsPendingBooking()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateAsync_ValidSomeBooking_ReturnsPendingBooking));
        var eventService = _dbContextMocker.ArrangeEventServiceTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, eventService, null);

        var result1 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var result2 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var result3 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();

        result1.Id.Should().NotBeEmpty();
        result2.Id.Should().NotBeEmpty();
        result3.Id.Should().NotBeEmpty();

        result1.Id.Should().NotBe(result2.Id);
        result1.Id.Should().NotBe(result3.Id);
        result2.Id.Should().NotBe(result3.Id);

        result1.EventId.Should().Be(eventId);
        result1.Status.Should().Be(BookingStatusEnum.Pending);
        result2.EventId.Should().Be(eventId);
        result2.Status.Should().Be(BookingStatusEnum.Pending);
        result3.EventId.Should().Be(eventId);
        result3.Status.Should().Be(BookingStatusEnum.Pending);
    }

    [Fact]
    public async Task GetBookingByIdAsync_GetValidBooking_ReturnsBooking()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.GetBookingByIdAsync_GetValidBooking_ReturnsBooking));
        var eventService = _dbContextMocker.ArrangeEventServiceTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, eventService, null);

        var bookingInfo = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        var result = await bookingService.GetBookingByIdAsync(bookingInfo.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(bookingInfo.Id);
        result.EventId.Should().Be(bookingInfo.EventId);
        result.Status.Should().Be(bookingInfo.Status);
        result.CreatedAt.Should().Be(bookingInfo.CreatedAt);
        result.ProcessedAt.Should().Be(bookingInfo.ProcessedAt);
    }

[Fact]
    public async Task ProcessPendingBookingAsync_GetConfirStatus_ReturnsBooking()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.ProcessPendingBookingAsync_GetConfirStatus_ReturnsBooking));
        var eventService = _dbContextMocker.ArrangeEventServiceTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, eventService, null);

        var bookingInfo = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        await bookingService.ProcessPendingBookingAsync(bookingInfo.Id, CancellationToken.None);

        var result = await bookingService.GetBookingByIdAsync(bookingInfo.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(bookingInfo.Id);
        result.EventId.Should().Be(bookingInfo.EventId);
        result.Status.Should().Be(BookingStatusEnum.Confirmed);
        result.CreatedAt.Should().Be(bookingInfo.CreatedAt);

        bookingInfo.ProcessedAt.Should().BeNull();
        result.ProcessedAt.Should().NotBeNull();
    }
}
