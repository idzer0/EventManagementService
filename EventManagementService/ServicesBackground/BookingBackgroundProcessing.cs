using EventManagementService.Contracts;
using EventManagementService.Models;
using EventManagementService.ServicesBackground;

namespace EventManagementService.ServicesBackground;

public class BookingBackgroundProcessing : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingBackgroundProcessing> _logger;

    public BookingBackgroundProcessing(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingBackgroundProcessing> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Сервис BookingBackgroundProcessing начал работу");

        while (!ct.IsCancellationRequested)
        {

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var _bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                foreach (var guid in await _bookingService.GetBookingIdsByStatusAsync(BookingStatusEnum.Pending, ct))
                {
                    // имитация бурной деятельности
                    await Task.Delay(2000, ct);
                    await _bookingService.ProcessPendingBookingAsync(guid, ct);
                }

                // Пауза перед следующим циклом
                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (KeyNotFoundException nfe)
            {
                _logger.LogWarning(nfe, "KeyNotFoundException при работе фонового процесса обработки бронирований.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при работе фонового процесса обработки бронирований.");
                // демпфер повторяющихся ошибок
                await Task.Delay(10000, ct);
            }
        }

        _logger.LogInformation("Сервис BookingBackgroundProcessing завершил работу");
    }
}