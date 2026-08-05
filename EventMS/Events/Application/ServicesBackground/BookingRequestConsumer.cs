using System.Text.Json;
using Application.Contracts;
using Application.DTO;
using Application.Services;
using Confluent.Kafka;
using Domain.DomainExceptions;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.ServicesBackground;

public class BookingRequestConsumer(
    ILogger<BookingRequestConsumer> logger,
    IOptions<KafkaSettings> settings,
    IServiceProvider serviceProvider,
    IEventService eventService) : KafkaConsumerService(logger, settings, serviceProvider)
{
    protected override async Task HandleMessageAsync(
        string key,
        string value,
        IServiceProvider scopeServiceProvider,
        CancellationToken ct)
    {
        // Проверить наличие мест, обновить AvailableSeats, сгенерировать ответ
        // и отправить ответное сообщение через IEventPublisher в топик OutgoingTopic
        var publisher = scopeServiceProvider.GetRequiredService<IEventPublisher>();
        var settings = scopeServiceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

        BookingRequest? bookingRequest = new();
        try
        {
            bookingRequest = JsonSerializer.Deserialize<BookingRequest>(value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось десериализовать сообщение:\n{Value}", value);
            return;
        }

        if (bookingRequest is null)
        {
            logger.LogError("Не удалось десериализовать сообщение:\n{Value}", value);
            return;
        }

        string message = string.Empty;
        bool isSuccess = false;
        try
        {
            isSuccess = await ProcessBookingAsync(bookingRequest, ct); // ваша бизнес-логика
        }
        catch(Exception ex)
        {
            message = ex.Message;
        }

        var response = new BookingResponse
        {
            BookingId = bookingRequest.BookingId,
            EventId = bookingRequest.EventId,
            BookingActionType = bookingRequest.BookingActionType,
            IsSuccess = isSuccess,
            ErrorMessage = isSuccess ? string.Empty : message,
            CreatedAt = DateTime.UtcNow,
        };

        await publisher.PublishAsync(settings.OutgoingTopic, key, JsonSerializer.Serialize(response));
    }

    /// <summary>
    /// Обработка запроса от сервиса бронирований: подтвердить или отменить бронирование.
    /// </summary>
    private async Task<bool> ProcessBookingAsync(BookingRequest request, CancellationToken ct)
    {
        bool result = request.BookingActionType == BookingActionTypeEnum.Confirm
            ? await eventService.ReserveSeat(request.EventId, ct)
            : await eventService.ReleaseSeat(request.EventId, ct);
        return result;
    }
}