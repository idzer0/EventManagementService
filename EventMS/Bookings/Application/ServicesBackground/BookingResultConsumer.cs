using System.Text.Json;
using Application.Contracts;
using Application.DTO;
using Application.Services;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.ServicesBackground;

public class BookingResultConsumer(
    ILogger<BookingResultConsumer> logger,
    IOptions<KafkaSettings> settings,
    IServiceProvider serviceProvider,
    IBookingResponseService service) : KafkaConsumerService(logger, settings, serviceProvider)
{

    /// <summary>
    /// Здесь завершаем процесс бронирования - обновляем статус)
    /// </summary>
    protected override async Task HandleMessageAsync(string key, string value, IServiceProvider scopeServiceProvider, CancellationToken ct)
    {
        BookingResponse? response = null;
        try
        {
            response = JsonSerializer.Deserialize<BookingResponse>(value);
        }
        catch (Exception ex)
        {
            logger.LogError($"Не удалось десериализовать сообщение: {value}", ex);
        }

        switch (response?.BookingActionType)
        {
            case BookingActionTypeEnum.Confirm:
                await service.ConfirmBookingAsync(response, ct);
                break;
            case BookingActionTypeEnum.Reject:
            case BookingActionTypeEnum.Cancel:
                await service.RejectBookingAsync(response, ct);
                break;
            case BookingActionTypeEnum.Delete:
                await service.DeleteBookingAsync(response, ct);
                break;
        }
    }
}
