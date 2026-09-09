using System.Text.Json;
using EventMS.Bookings.Application.Contracts;
using KafkaSettingsShared.DTO;
using KafkaSettingsShared.Enums;
using KafkaSettingsShared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventMS.Bookings.Application.ServicesBackground;

public class BookingResultConsumer(
    ILogger<BookingResultConsumer> logger,
    IOptions<KafkaSettings> settings,
    IServiceProvider serviceProvider) : KafkaConsumerService(logger, settings, serviceProvider)
{

    /// <summary>
    /// Здесь завершаем процесс бронирования - обновляем статус)
    /// </summary>
    protected override async Task HandleMessageAsync(string key, string value, IServiceProvider scopeServiceProvider, CancellationToken ct)
    {
        var service = scopeServiceProvider.GetRequiredService<IBookingResponseService>();

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
