using EventMS.Bookings.Application.DTO;
using EventMS.Bookings.Domain.DomainExceptions;
using EventMS.Bookings.Domain.Models;
using EventMS.Bookings.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventMS.Bookings.Bookings.Tests;

[Collection("Конкурентные сценарии сервиса бронирования")]
public class BookingServiceConcurentTests
{
    private readonly DbContextMocker _dbContextMocker;

    public BookingServiceConcurentTests ()
    {
        _dbContextMocker = new DbContextMocker();
    }

    [Fact]
    public async Task ProcessPendingBookingAsync_ConcurentProcess_AllPositive()
    {
        Guid eventId = Guid.NewGuid();

        var dbContext = _dbContextMocker.GetAppDbContext(nameof(this.ProcessPendingBookingAsync_ConcurentProcess_AllPositive));
        var bookingService = _dbContextMocker.ArrangeBookingServiceTestCase(dbContext, []);

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
        int countProcessed = processedBookings.Select(book => book.Status == BookingStatusEnum.InProcessing).Count();

        Assert.Equal(10, countUnqueId);
        Assert.Equal(10, countProcessed);
    }
}
