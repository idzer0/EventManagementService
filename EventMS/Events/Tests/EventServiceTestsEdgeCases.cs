using EventMS.Events.Application.Contracts;
using EventMS.Events.Application.DTO;
using EventMS.Events.Domain.DomainExceptions;
using EventMS.Events.Domain.Models;
using Events.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventMS.Events.Tests;

[Collection("Граничные случаи")]
public class EventServiceTestsEdgeCases
{
    private readonly DbContextMocker _dbContextMocker;
    private readonly Mock<ICacheService> _cache;
    private readonly Mock<IRedisCacheService> _redis;
    public EventServiceTestsEdgeCases ()
    {
        _dbContextMocker = new DbContextMocker();
        _redis = new Mock<IRedisCacheService>();
        _cache = new Mock<ICacheService>();
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_EmptyTitleFilter_ReturnsAllEvents()
    {
        var events = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Событие 1", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "Событие 2", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) }
        };

        var filter = new EventsFilter { Title = "" };

        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_EmptyTitleFilter_ReturnsAllEvents)),
            _redis.Object,
            _cache.Object,
            events);

        var result = await service.GetPaginatedEventsAsync(filter, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_DateFilterWithNullValues_ReturnsAllEvents()
    {
        var events = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Событие 1", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "Событие 2", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) }
        };

        var filter = new EventsFilter { From = null, To = null };

        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_DateFilterWithNullValues_ReturnsAllEvents)),
            _redis.Object,
            _cache.Object,
            events);

        var result = await service.GetPaginatedEventsAsync(filter, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_PageLessThanOne_UsesDefaultValue()
    {
        var filter = new EventsFilter { Page = 0, PageSize = 1 }; // Невалидный номер страницы

        var events = new List<EventEntity>();
        for (int i = 1; i <= 5; i++)
        {
            events.Add(new EventEntity
            {
                Id = Guid.NewGuid(),
                Title = $"Event {i}",
                StartAt = DateTime.UtcNow.AddDays(i),
                EndAt = DateTime.UtcNow.AddDays(i + 1)
            });
        }

        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_PageSizeLessThanOne_UsesDefaultValue)),
            _redis.Object,
            _cache.Object,
            events);

        Func<Task> act = async () => await service.GetPaginatedEventsAsync(filter, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationDomainException>()
            .WithMessage("Номер страницы и размер страницы не могут быть равны нулю.");
    }

   [Fact]
    public async Task GetPaginatedEventsAsync_PageSizeLessThanOne_UsesDefaultValue()
    {
        var filter = new EventsFilter { Page = 1, PageSize = 0 }; // Невалидный размер страницы

        var events = new List<EventEntity>();
        for (int i = 1; i <= 5; i++)
        {
            events.Add(new EventEntity
            {
                Id = Guid.NewGuid(),
                Title = $"Event {i}",
                StartAt = DateTime.UtcNow.AddDays(i),
                EndAt = DateTime.UtcNow.AddDays(i + 1)
            });
        }

        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_PageSizeLessThanOne_UsesDefaultValue)),
            _redis.Object,
            _cache.Object,
            events);

        Func<Task> act = async () => await service.GetPaginatedEventsAsync(filter, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationDomainException>()
            .WithMessage("Номер страницы и размер страницы не могут быть равны нулю.");
    }
}