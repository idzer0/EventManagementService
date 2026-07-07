using Domain.DomainExceptions;
using Domain.Models;
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
            TotalSeats = 100,
            AvailableSeats = 100,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateAsync_ValidOneBooking_ReturnsPendingBooking));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, null);

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
            TotalSeats = 100,
            AvailableSeats = 100,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateAsync_ValidSomeBooking_ReturnsPendingBooking));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, null);

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
            TotalSeats = 100,
            AvailableSeats = 100,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.GetBookingByIdAsync_GetValidBooking_ReturnsBooking));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, null);

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
    public async Task ProcessPendingBookingAsync_GetConfirmStatus_ReturnsBooking()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event 1",
            Description = "Test event 1",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
            TotalSeats = 100,
            AvailableSeats = 100,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.ProcessPendingBookingAsync_GetConfirmStatus_ReturnsBooking));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, null);

        var bookingInfo = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        await bookingService.ProcessPendingBookingAsync(bookingInfo.Id, CancellationToken.None);
        var result = await bookingService.GetBookingByIdAsync(bookingInfo.Id, CancellationToken.None);
        var resultEvent = await repoEvents.GetByIdAsync(eventId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(bookingInfo.Id);
        result.EventId.Should().Be(bookingInfo.EventId);
        result.Status.Should().Be(BookingStatusEnum.Confirmed);
        result.CreatedAt.Should().Be(bookingInfo.CreatedAt);

        bookingInfo.ProcessedAt.Should().BeNull();
        result.ProcessedAt.Should().NotBeNull();

        #pragma warning disable CS8602
        resultEvent.AvailableSeats.Should().Be(99);
        #pragma warning restore CS8602
    }

    [Fact]
    public async Task CreateBookingAsync_CheckSeatsLimit_ReturnsThrow()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test limit event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
            TotalSeats = 2,
            AvailableSeats = 2,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateBookingAsync_CheckSeatsLimit_ReturnsThrow));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, null);

        var bookingInfo1 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var bookingInfo2 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        Func<Task> bookingInfo3 = async () => await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        bookingInfo1.Should().NotBeNull();
        bookingInfo2.Should().NotBeNull();
        bookingInfo1.Id.Should().NotBe(bookingInfo2.Id);
        bookingInfo1.EventId.Should().Be(bookingInfo2.EventId);

        await bookingInfo3.Should().ThrowAsync<NoAvailableSeatsDomainException>()
            .WithMessage("No available seats for this event");
    }

    [Fact]
    public async Task RejectAsync_RejectAndThenCreate_CheckAvaliableSeats()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test limit event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
            TotalSeats = 2,
            AvailableSeats = 2,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.RejectAsync_RejectAndThenCreate_CheckAvaliableSeats));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, null);

        var bookingInfo1 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var bookingInfo2 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        await bookingService.RejectAsync(bookingInfo1.Id, CancellationToken.None);
        var bookingInfo3 = await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        var bookingInfo1_1 = await bookingService.GetBookingByIdAsync(bookingInfo1.Id, CancellationToken.None);

        bookingInfo1.Should().NotBeNull();
        bookingInfo2.Should().NotBeNull();
        bookingInfo3.Should().NotBeNull();
        bookingInfo1.Id.Should().NotBe(bookingInfo2.Id);
        bookingInfo1_1.EventId.Should().Be(bookingInfo2.EventId);
        bookingInfo1_1.Status.Should().Be(BookingStatusEnum.Rejected);
        bookingInfo3.EventId.Should().Be(bookingInfo1_1.EventId);
        bookingInfo3.Status.Should().Be(BookingStatusEnum.Pending);
    }
}
