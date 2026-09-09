using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using EventMS.Bookings.Application.DTO;
using EventMS.Bookings.Domain.Models;
using EventMS.Events.Application.DTO;
using FluentAssertions;
using Polly;
using Xunit;
using Xunit.Abstractions;

namespace EventMS.E2ETests.E2ETestsSimple;

public class BookingE2ETests
{
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;

    // Адреса сервисов (можно вынести в конфигурацию)
    private const string AuthBaseUrl = "http://localhost:5001";
    private const string EventsBaseUrl = "http://localhost:5003";
    private const string BookingsBaseUrl = "http://localhost:5002";

    public BookingE2ETests(ITestOutputHelper output)
    {
        _output = output;
        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task FullBookingFlow_ShouldCompleteSuccessfully()
    {
        // 1. Регистрация пользователя
        var login = $"usr-{Guid.NewGuid()}";
        var password = "Password123!";
        var registerRequest = new
        {
            Login = login,
            Password = password,
            Role = 2 // Роль: User
        };

        var registerResponse = await _httpClient.PostAsJsonAsync(
            $"{AuthBaseUrl}/api/auth/register", registerRequest);

        // Ожидаем 200 OK (контроллер возвращает Ok() без тела)
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Получение токена (предполагаем наличие endpoint /api/auth/login)
        var loginRequest = new
        {
            Login = login,
            Password = password
        };

        var loginResponse = await _httpClient.PostAsJsonAsync(
            $"{AuthBaseUrl}/api/auth/login", loginRequest);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authContent = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var token = authContent!.Token;

        // 3. Создание события
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var eventRequest = new EventRequest()
        {
            Title = "E2E Test Event",
            Description = "E2E Test Event",
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(3),
            TotalSeats = 50,
            AvailableSeats = 50,
        };

        var eventResponse = await _httpClient.PostAsJsonAsync(
            $"{EventsBaseUrl}/events", eventRequest);
        eventResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var eventContent = await eventResponse.Content.ReadFromJsonAsync<EventResponse>();
        var eventId = eventContent!.Id;

        // 4. Создание бронирования
        var bookingResponse = await _httpClient.PostAsync(
            $"{BookingsBaseUrl}/bookings/{eventId}/book", null);
        bookingResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);  // 202 Accepted
        var bookingContent = await bookingResponse.Content.ReadFromJsonAsync<BookingInfo>();
        var bookingId = bookingContent!.Id;

        // 5. Ожидание подтверждения (асинхронная обработка через Kafka)
                await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 10,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(2),
                onRetry: (exception, timespan, retryNo, _) =>
                {
                    _output.WriteLine($"Retry {retryNo}: booking {bookingId} is not confirmed yet.");
                })
            .ExecuteAsync(async () =>
            {
                var statusResponse = await _httpClient.GetAsync(
                    $"{BookingsBaseUrl}/bookings/{bookingId}");
                statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                var status = await statusResponse.Content.ReadFromJsonAsync<BookingInfo>();
                status!.Status.Should().Be(BookingStatusEnum.Confirmed);
            });

        _output.WriteLine($"Booking {bookingId} confirmed successfully.");
    }

    // DTO для ответов
    private class AuthResponse { public string Token { get; set; } = string.Empty; }
}
