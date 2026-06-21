using EventManagementService.Models;
using EventManagementService.Services;
using EventManagementServiceTestsDb.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTestsDb;

[Collection("PostgresCollection")]
public class EventRepositoryTests (PostgresFixture fixture) : UnitDBTestBase(fixture)
{
    [Fact]
    public async Task CreateAsync_CreatedEventExist_ReturnOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        Guid eventId = Guid.NewGuid();

        var ev = TestDataHelper.GetEventEntity(eventId);

        var repo = new EventRepository(context, NullLogger<EventRepository>.Instance);

        // Act
        _ = await repo.CreateAsync(ev, CancellationToken.None);
        EventEntity? result = await repo.GetByIdAsync(eventId, CancellationToken.None);

        // Assert
        result?.Id.Should().Be(eventId);
    }

    [Fact]
    public async Task GetPaginatedEventsAsync_CheckOrderBy_ReturnsOrderList()
    {
        //Arrange
        var events = TestDataHelper.GetListEventEntity(false, 24);

        var filter = new EventsFilter { Page = 1, PageSize = 10 };

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();

        var repo = new EventRepository(context, NullLogger<EventRepository>.Instance);

        // Act
        var notOrderedResult = (List<EventEntity>)await repo.GetAllAsync(CancellationToken.None);
        var orderedResult = await repo.GetPaginatedEventsAsync(filter, CancellationToken.None);

        // Assert
        notOrderedResult[0].Title.Should().NotBe("Событие 24");
        notOrderedResult.Count.Should().Be(24);
        orderedResult[0].Title.Should().Be("Событие 24");
        orderedResult.Count.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_CheckSavedData_ReturnsUpdatedEvent()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repo = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var ev = await repo.CreateAsync(TestDataHelper.GetEventEntity(), CancellationToken.None);

        // Act
        ev.Title = "new title";
        var result = await repo.UpdateAsync(ev, CancellationToken.None);

        // Assert
        result.Title.Should().Be("new title");
        result.Id.Should().Be(ev.Id);
    }

    [Fact]
    public async Task DeleteAsync_CheckDeletedData_DeleteOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repo = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var ev = await repo.CreateAsync(TestDataHelper.GetEventEntity(), CancellationToken.None);
        Guid eventId = ev.Id;

        // Act
        var resultDelete = await repo.DeleteAsync(eventId, CancellationToken.None);
        var resultCheck = await repo.IsExistsAsync(eventId, CancellationToken.None);

        // Assert
        resultDelete.Should().Be(true);
        resultCheck.Should().Be(false);
    }

    [Fact]
    public async Task EventsCountAsync_CheckReturnValue_ReturnsCount()
    {
        //Arrange
        var events1 = TestDataHelper.GetListEventEntity(false, 5, numSeats: 10);
        var events2 = TestDataHelper.GetListEventEntity(false, 12, DateTime.UtcNow.AddDays(60), 50);

        var filter = new EventsFilter { Page = 1, PageSize = 10, From = DateTime.UtcNow.AddDays(20)};

        await ResetDatabaseAsync();
        await using var context = CreateContext();
        await context.Events.AddRangeAsync(events1);
        await context.Events.AddRangeAsync(events2);
        await context.SaveChangesAsync();

        var repo = new EventRepository(context, NullLogger<EventRepository>.Instance);

        // Act
        var countAll = await repo.EventsCountAsync(new EventsFilter(), CancellationToken.None);
        var countFilter = await repo.EventsCountAsync(filter, CancellationToken.None);

        // Assert
        countAll.Should().Be(17);
        countFilter.Should().Be(12);
    }

    [Fact]
    public async Task SelectTiltleByLike_UseTrgmIndex_ReturnCorrectRows()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repo = new EventRepository(context, NullLogger<EventRepository>.Instance);
        var ev0 = await repo.CreateAsync(TestDataHelper.GetEventEntity(), CancellationToken.None);
        var ev1 = await repo.CreateAsync(TestDataHelper.GetEventEntity(title: "Фестиваль цветов Ромашка"), CancellationToken.None);
        var ev2 = await repo.CreateAsync(TestDataHelper.GetEventEntity(title: "Цвет - отражение настроения"), CancellationToken.None);
        var ev3 = await repo.CreateAsync(TestDataHelper.GetEventEntity(title: "Выставка: Всё в цвет"), CancellationToken.None);
        var ev4 = await repo.CreateAsync(TestDataHelper.GetEventEntity(), CancellationToken.None);

        var filter = new EventsFilter()
        {
            Title = "цвет"
        };

        // Act
        var result = await repo.GetPaginatedEventsAsync(filter, CancellationToken.None);

        // Assert
        result.Count.Should().Be(3);
        result.Exists(ev => ev.Id == ev1.Id).Should().BeTrue();
        result.Exists(ev => ev.Id == ev2.Id).Should().BeTrue();
        result.Exists(ev => ev.Id == ev3.Id).Should().BeTrue();
    }
}