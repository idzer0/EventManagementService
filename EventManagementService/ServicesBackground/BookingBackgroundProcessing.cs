using EventManagementService.Contracts;
using EventManagementService.Models;
using EventManagementService.ServicesBackground;

namespace EventManagementService.ServicesBackground;

public class BookingBackgroundProcessing : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingBackgroundProcessing> _logger;

    private readonly int maxConcurrency = Environment.ProcessorCount;

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

                var guids = await _bookingService.GetBookingIdsByStatusAsync(BookingStatusEnum.Pending, ct, maxConcurrency);

                await Task.WhenAll(guids.Select(
                    async guid => await ProcessPendingBookingAsync(_bookingService, guid, ct)
                ));


                // Пауза перед следующим циклом
                await Task.Delay(5000, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (KeyNotFoundException nfe)
            {
                _logger.LogWarning(nfe, "KeyNotFoundException при работе фонового процесса обработки бронирований.");
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    _logger.LogError(ex, "Ошибка при обработке бронирований.");
                }
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

    private async Task ProcessPendingBookingAsync(IBookingService service, Guid guid, CancellationToken ct)
    {
        await Task.Delay(2000, ct);

        await service.ProcessPendingBookingAsync(guid, ct);
    }
}