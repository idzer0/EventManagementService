using EventManagementService.Contracts;
using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementServiceTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManagementServiceTests;

[Collection("Граничные случаи")]
public class EventServiceTestsEdgeCases
{
    DbContextMocker _dbContextMocker;

    public EventServiceTestsEdgeCases ()
    {
        _dbContextMocker = new DbContextMocker();
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
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_EmptyTitleFilter_ReturnsAllEvents)), events);

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
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_PageSizeLessThanOne_UsesDefaultValue)), events);

        Func<Task> act = async () => await service.GetPaginatedEventsAsync(filter, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
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
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_PageSizeLessThanOne_UsesDefaultValue)), events);

        Func<Task> act = async () => await service.GetPaginatedEventsAsync(filter, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Номер страницы и размер страницы не могут быть равны нулю.");
    }
}