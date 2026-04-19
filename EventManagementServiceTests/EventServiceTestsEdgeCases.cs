using Moq;
using EventManagementService.Contracts;
using EventManagementService.Models;
using FluentAssertions;
using EventManagementService.Services;
//using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using EventManagementServiceTests.Infrastructure;

namespace EventManagementServiceTests;

[Collection("Граничные случаи")]
public class EventServiceTestsEdgeCases
{
    EventsDbContextMocker _dbContextMocker;

    public EventServiceTestsEdgeCases ()
    {
        _dbContextMocker = new EventsDbContextMocker();
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
            nameof(this.GetPaginatedEventsAsync_EmptyTitleFilter_ReturnsAllEvents), events);

        
        var result = await service.GetPaginatedEventsAsync(filter);
        
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
            nameof(this.GetPaginatedEventsAsync_DateFilterWithNullValues_ReturnsAllEvents), events);

        var result = await service.GetPaginatedEventsAsync(filter);
        
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
    
    [Fact]
    public async Task GetPaginatedEventsAsync_PageSizeLessThanOne_UsesDefaultValue()
    {
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
        
        var filter = new EventsFilter { Page = 1, PageSize = 0 }; // Невалидный размер страницы
        
        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            nameof(this.GetPaginatedEventsAsync_PageSizeLessThanOne_UsesDefaultValue), events);

        var result = await service.GetPaginatedEventsAsync(filter);

        result.Items.Should().NotBeNull();
    }
}