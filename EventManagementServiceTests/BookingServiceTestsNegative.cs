using EventManagementService.Contracts;
using EventManagementService.DomainExceptions;
using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementService.ServicesBackground;
using EventManagementServiceTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTests;

[Collection("Негативные сценарии сервиса бронирования")]
public class BookingServiceFailTests
{
    private readonly DbContextMocker _dbContextMocker;

    public BookingServiceFailTests ()
    {
        _dbContextMocker = new DbContextMocker();
    }

    [Fact]
    public async Task CreateAsync_MissingEvent_ThrowsObjectNotFoundDomainException()
    {
        Guid eventId = Guid.NewGuid();

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateAsync_MissingEvent_ThrowsObjectNotFoundDomainException));
        var eventService = _dbContextMocker.ArrangeEventServiceTestCase(dbContext, null);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, eventService, null);

        Func<Task> act = async () => await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        await act.Should().ThrowAsync<ObjectNotFoundDomainException>()
            .WithMessage($"События с Id {eventId} не найдено.");
    }

    [Fact]
    public async Task CreateAsync_ForDeletedEvent_ThrowsObjectNotFoundDomainException()
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

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateAsync_ForDeletedEvent_ThrowsObjectNotFoundDomainException));
        var eventService = _dbContextMocker.ArrangeEventServiceTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, eventService, null);

        await eventService.DeleteAsync(eventId, CancellationToken.None);
        Func<Task> act = async () => await bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        await act.Should().ThrowAsync<ObjectNotFoundDomainException>()
            .WithMessage($"События с Id {eventId} не найдено.");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsObjectNotFoundDomainException()
    {
        Guid Id = Guid.NewGuid();

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.GetByIdAsync_NonExistingId_ThrowsObjectNotFoundDomainException));
        var eventService = _dbContextMocker.ArrangeEventServiceTestCase(dbContext, null);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, eventService, null);

        Func<Task> act = async () => await bookingService.GetBookingByIdAsync(Id, CancellationToken.None);

        await act.Should().ThrowAsync<ObjectNotFoundDomainException>()
            .WithMessage($"Бронь с Id {Id} не найдена.");
    }
}
