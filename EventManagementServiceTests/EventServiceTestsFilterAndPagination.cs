using Moq;
using EventManagementService.Contracts;
using EventManagementService.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using EventManagementService.Services;
using EventManagementService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using EventManagementServiceTests.Infrastructure;

namespace EventManagementServiceTests;

[Collection("Фильтрация и пагинация")]
public class EventServiceTestsFilterAndPagination
{
    private readonly DbContextMocker _dbContextMocker;

    public EventServiceTestsFilterAndPagination ()
    {
        _dbContextMocker = new DbContextMocker();
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_FilterByTitle_ReturnsFilteredEvents()
    {

        var events = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Музыкальный фестиваль", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "Техническая конференция", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "Встреча 1 to 1", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) }
        };

        var repo = _dbContextMocker.ArrangeEventsRepositoryTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_FilterByTitle_ReturnsFilteredEvents)), events);

        var filter = new EventsFilter { Title = "Музыкальный" };

        var result = await repo.GetPaginatedEventsAsync(filter, CancellationToken.None);

        result.Should().ContainSingle(e => e.Title.Contains("Музыкальный"));
        result.Should().NotContain(e => e.Title.Contains("Технич"));
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_FilterByDateRange_ReturnsFilteredEvents()
    {
        var now = DateTime.UtcNow;
        var events = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Событие 1", StartAt = now.AddDays(1), EndAt = now.AddDays(2) },
            new() { Id = Guid.NewGuid(), Title = "Событие 2", StartAt = now.AddDays(3), EndAt = now.AddDays(4) },
            new() { Id = Guid.NewGuid(), Title = "Событие 3", StartAt = now.AddDays(5), EndAt = now.AddDays(6) }
        };

        var filter = new EventsFilter
        {
            From = now.AddDays(2),
            To = now.AddDays(5)
        };

        var repo = _dbContextMocker.ArrangeEventsRepositoryTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_FilterByDateRange_ReturnsFilteredEvents)), events);

        var result = await repo.GetPaginatedEventsAsync(filter, CancellationToken.None);

        result.Should().Contain(e => e.Title == "Событие 2");
        result.Should().NotContain(e => e.Title == "Событие 1");
        result.Should().NotContain(e => e.Title == "Событие 3");
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_WithPagination_ReturnsCorrectPage()
    {
        var events = new List<EventEntity>();
        for (int i = 1; i <= 25; i++)
        {
            events.Add(new EventEntity
            {
                Id = Guid.NewGuid(),
                Title = $"Событие {i}",
                StartAt = DateTime.UtcNow.AddDays(i),
                EndAt = DateTime.UtcNow.AddDays(i + 1)
            });
        }

        var filter = new EventsFilter { Page = 2, PageSize = 10 };

        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_WithPagination_ReturnsCorrectPage)), events);

        var result = await service.GetPaginatedEventsAsync(filter, CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.Items.Should().HaveCount(10);
        result.Items[0].Title.Should().Be("Событие 11");
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_CombinedFilters_ReturnsCorrectResults()
    {
        var events = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Встреча 1", StartAt = DateTime.UtcNow.AddDays(4), EndAt = DateTime.UtcNow.AddDays(5) },
            new() { Id = Guid.NewGuid(), Title = "Закрытие конференции", StartAt = DateTime.UtcNow.AddDays(5), EndAt = DateTime.UtcNow.AddDays(6) },
            new() { Id = Guid.NewGuid(), Title = "Фуршет", StartAt = DateTime.UtcNow.AddDays(1), EndAt = DateTime.UtcNow.AddDays(2) }
        };

        var filter = new EventsFilter
        {
            Title = "Встреча",
            From = DateTime.UtcNow.AddDays(4).Date,
            To = DateTime.UtcNow.AddDays(7).Date
        };

        var repo = _dbContextMocker.ArrangeEventsRepositoryTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.GetPaginatedEventsAsync_CombinedFilters_ReturnsCorrectResults)), events);

        var result = await repo.GetPaginatedEventsAsync(filter, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Title.Should().Be("Встреча 1");
    }
}