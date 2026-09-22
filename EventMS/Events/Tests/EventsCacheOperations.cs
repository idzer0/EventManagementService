using System.Text.Json;
using EventMS.Events.Application.Contracts;
using EventMS.Events.Application.DTO;
using EventMS.Events.Application.Mappers;
using EventMS.Events.Application.Services;
using EventMS.Events.Domain.Models;
using Events.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;


// using Events.Tests.Infrastructure;
// using FluentAssertions;
// using Infrastructure.Services;
// using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static EventMS.Events.Application.Services.RedisValueKeys;

namespace EventMS.Events.Tests;

[Collection("Операции с кешем")]

public class EventsCacheOperations
{
    private readonly DbContextMocker _dbContextMocker;

    public EventsCacheOperations ()
    {
        _dbContextMocker = new DbContextMocker();
    }

    [Fact]
    public async Task GetTopSaledAsync_WhenCacheHasData_ReturnsCachedData()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var cachedEvents = new[] { new EventEntity {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 100,
            AvailableSeats = 100 }
        };
        var cachedJson = JsonSerializer.Serialize(cachedEvents);

        var redisMock = new Mock<IRedisCacheService>();
        redisMock.Setup(c => c.GetValueAsync(RedisValueKeys.Top10Saled, It.IsAny<int>()))
                 .ReturnsAsync(cachedJson);

        var cache = new CacheService(redisMock.Object, NullLogger<CacheService>.Instance);
        var repoMock = new Mock<IEventRepository>();

        // Репозиторий не должен вызываться, но если вызовется, то тест провалится
        repoMock.Setup(r => r.GetTopSaledAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Repository should not be called"));

        var service = new EventService(repoMock.Object, cache, NullLogger<EventService>.Instance);

        // Act
        var result = await service.GetTopSaledAsync(CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(eventId, result[0].Id);

        // Проверяем, что репозиторий не вызывался
        repoMock.Verify(r => r.GetTopSaledAsync(It.IsAny<CancellationToken>()), Times.Never);
        // Проверяем, что SetValueAsync не вызывался
        redisMock.Verify(c => c.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<int>()), Times.Never);
    }

[Fact]
    public async Task GetTopSaledAsync_WhenCacheEmpty_LoadsFromRepositoryAndCaches()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var eventsFromDb = new EventEntity {
            Id = eventId,
            Title = "Тестовое событие",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 100,
            AvailableSeats = 100
        };
        var expectedJson = JsonSerializer.Serialize(eventsFromDb);

        var redisMock = new Mock<IRedisCacheService>();
        redisMock.Setup(c => c.GetValueAsync(RedisValueKeys.EventKey(eventId), It.IsAny<int>()))
                 .ReturnsAsync((string?)null); // нет кеша
        redisMock.Setup(c => c.SetValueAsync(RedisValueKeys.EventKey(eventId), expectedJson, It.IsAny<TimeSpan?>(), It.IsAny<int>()))
                 .ReturnsAsync(true);

        var cache = new CacheService(redisMock.Object, NullLogger<CacheService>.Instance);

        var repoMock = new Mock<IEventRepository>();
        repoMock.Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventsFromDb);

        var service = new EventService(repoMock.Object, cache, NullLogger<EventService>.Instance);

        // Act
        var result = await service.GetByIdAsync(eventId, It.IsAny<CancellationToken>());

        // Assert
        Assert.Equal(eventId, result.Id);
        // Проверяем, что репозиторий вызывался один раз
        repoMock.Verify(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
        redisMock.Verify(c => c.SetValueAsync(RedisValueKeys.EventKey(eventId), expectedJson, It.IsAny<TimeSpan?>(), It.IsAny<int>()), Times.Once);
    }


    [Fact]
    public async Task UpdateEventAsync_InvalidatesCache()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var eventRequest = new EventRequest() {
            Title = "Тестовое событие",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 100,
            AvailableSeats = 100
        };

        var updatedEvent = EventMapper.MapToEntity(eventId, eventRequest);

        var redisMock = new Mock<IRedisCacheService>();
        redisMock.Setup(c => c.RemoveKeyAsync(RedisValueKeys.EventKey(eventId), It.IsAny<int>()))
                 .ReturnsAsync(true);

        var cache = new CacheService(redisMock.Object, NullLogger<CacheService>.Instance);

        var context = _dbContextMocker.GetAppDbContext(nameof(this.UpdateEventAsync_InvalidatesCache));
        var repo = _dbContextMocker.ArrangeEventsRepositoryTestCase(
            context,
            redisMock.Object,
            [updatedEvent]);
        context.Entry(updatedEvent).State = EntityState.Detached;

        var service = new EventService(repo, cache, NullLogger<EventService>.Instance);

        // Act
        await service.UpdateAsync(eventId, eventRequest, CancellationToken.None);

        // Assert
        // Проверяем, что репозиторий вызывался один раз
        redisMock.Verify(c => c.RemoveKeyAsync(RedisValueKeys.EventKey(eventId), It.IsAny<int>()), Times.Once);
    }

}
