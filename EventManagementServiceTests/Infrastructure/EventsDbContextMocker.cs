using EventManagementService.Contracts;
using EventManagementService.Infrastructure;
using EventManagementService.Models;
using EventManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTests.Infrastructure;

public class EventsDbContextMocker()
{
    private static AppDbContext GetAppDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var dbContext = new AppDbContext(options);

        return dbContext;
    }

    public IEventRepository ArrangeRepositoryTestCase(string dbName, List<EventEntity> items)
    {
        var dbContext = GetAppDbContext(dbName);
        dbContext.Events.AddRange(items);

        return new EventRepository(dbContext, NullLogger<EventRepository>.Instance);
    }

    public IEventService ArrangeEventServiceTestCase(
        string dbName, 
        List<EventEntity> items)
    {
        var dbContext = GetAppDbContext(dbName);
        dbContext.Events.RemoveRange(dbContext.Events.ToArray());
        dbContext.SaveChanges();
        
        dbContext.Events.AddRange(items);
        dbContext.SaveChanges();

        IEventRepository repo =  new EventRepository(dbContext, NullLogger<EventRepository>.Instance);

        return new EventService(repo, NullLogger<EventService>.Instance);
    }
}