using EventManagementService.Contracts;
using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementServiceTestsDb.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTestsDb;

[Collection("Успешные операции c репозиторием EventsRepository")]
public class EventRepoTests : UnitDBTestBase
{
    [Fact]
    public async Task CreateAsync_CreatedEventExist()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        Guid eventId = Guid.NewGuid();

        var ev = new EventEntity
        {
            Id = eventId,
            Title = "Test event",
            Description = "Test event",
            StartAt = DateTime.UtcNow.Date.AddDays(1),
            EndAt = DateTime.UtcNow.Date.AddDays(2),
            TotalSeats = 5,
            AvailableSeats = 5,
        };


        context.Events.Add(ev);
        context.SaveChanges();


        EventEntity? result = await context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        result?.Id.Should().Be(result.Id);
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_CheckOrderBy_ReturnsOrderList()
    {
        var events = new List<EventEntity>();
        for (int i = 1; i <= 24; i++)
        {
            events.Add(new EventEntity
            {
                Id = Guid.NewGuid(),
                Title = $"Событие {i}",
                StartAt = i == 24 ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(i),
                EndAt = DateTime.UtcNow.AddDays(i + 1)
            });
        }

        var filter = new EventsFilter { Page = 1, PageSize = 10 };

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        var repo = new EventRepository(context, NullLogger<EventRepository>.Instance);

        var notOrderedResult = (List<EventEntity>)await repo.GetAllAsync(CancellationToken.None);
        var orderedResult = await repo.GetPaginatedEventsAsync(filter, CancellationToken.None);

        notOrderedResult[0].Title.Should().NotBe("Событие 24");
        notOrderedResult.Count.Should().Be(24);
        orderedResult[0].Title.Should().Be("Событие 24");
        orderedResult.Count.Should().Be(10);
    }

}