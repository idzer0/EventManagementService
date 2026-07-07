using Application.DTO;
using Domain.DomainExceptions;
using Domain.Models;
using EventManagementServiceTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTests;

[Collection("Конкурентные сценарии сервиса бронирования")]
public class BookingServiceConcurentTests
{
    private readonly DbContextMocker _dbContextMocker;

    public BookingServiceConcurentTests ()
    {
        _dbContextMocker = new DbContextMocker();
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurentTest_ForFiveSeats()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
            TotalSeats = 5,
            AvailableSeats = 5,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.CreateBookingAsync_ConcurentTest_ForFiveSeats));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, []);

        List<Task> tasks = [];

        for (int i = 0; i < 20; i++)
            tasks.Add(bookingService.CreateBookingAsync(eventId, CancellationToken.None));

        var allTasks = Task.WhenAll(tasks);

        try
        {
            await allTasks.ContinueWith(_ => { });
        }
        catch
        {
            #pragma warning disable CS8602
            var allExceptions = allTasks.Exception.InnerExceptions;
            #pragma warning restore CS8602

            Assert.Equal(15, allExceptions.Count);
            Assert.Contains(allExceptions, e => e is ArgumentException);
            Assert.Contains(allExceptions, e => e is NoAvailableSeatsDomainException);
        }

        int isCompletedSuccessfullyCount = tasks.Count(t => t.IsCompletedSuccessfully);
        int isFaultedCount = tasks.Count(t => t.IsFaulted);

        Assert.Equal(5, isCompletedSuccessfullyCount);
        Assert.Equal(15, isFaultedCount);
        Assert.Equal(0, ev.AvailableSeats);
    }

    [Fact]
    public async Task ProcessPendingBookingAsync_ConcurentProcess_AllPositive()
    {
        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test event",
            StartAt = DateTime.Now.Date.AddDays(1),
            EndAt = DateTime.Now.Date.AddDays(2),
            TotalSeats = 10,
            AvailableSeats = 10,
        };

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.ProcessPendingBookingAsync_ConcurentProcess_AllPositive));
        var repoEvents = _dbContextMocker.ArrangeEventsRepositoryTestCase(dbContext, [ev]);
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, repoEvents, []);

        List<Task<BookingInfo>> tasks = [];

        for (int i = 0; i < 10; i++)
            tasks.Add(bookingService.CreateBookingAsync(eventId, CancellationToken.None));

        // Созданные бронирования
        BookingInfo[] bookings = await Task.WhenAll(tasks);

        // Обрабатываем бронирования
        List<Task> processingTasks = [];
        processingTasks.AddRange(bookings.Select(t =>
            bookingService.ProcessPendingBookingAsync(t.Id, CancellationToken.None)));

        await Task.WhenAll(processingTasks);

        // Получаем результат обработки
        var gettingBookingTasks = bookings.Select(book =>
            bookingService.GetBookingByIdAsync(book.Id, CancellationToken.None));

        BookingInfo[] processedBookings = await Task.WhenAll(gettingBookingTasks);

        int countUnqueId = processedBookings.Select(book => book.Id).Distinct().Count();
        int countProcessed = processedBookings.Select(book => book.Status == BookingStatusEnum.Confirmed).Count();

        Assert.Equal(10, countUnqueId);
        Assert.Equal(10, countProcessed);
    }
}
