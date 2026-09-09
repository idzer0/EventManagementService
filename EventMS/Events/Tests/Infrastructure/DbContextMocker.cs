using EventMS.Events.Application.Contracts;
using EventMS.Events.Application.Services;
using EventMS.Events.Domain.Models;
using EventMS.Events.Infrastructure.DataAccess;
using EventMS.Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Events.Tests.Infrastructure;

public class DbContextMocker()
{
    public AppDbContext GetAppDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        AppDbContext context = new(options);
        PrepareDataDbContext(context);

        return context;
    }

    public IEventRepository ArrangeEventsRepositoryTestCase(AppDbContext dbContext, IRedisCacheService redis, List<EventEntity> items)
    {
        AddItemsToDbContext(dbContext, items);
        return new EventRepository(dbContext, redis, NullLogger<EventRepository>.Instance);
    }

    public IEventService ArrangeEventServiceTestCase(
        AppDbContext dbContext,
        IRedisCacheService redis,
        ICacheService cashe,
        List<EventEntity>? items)
    {
        AddItemsToDbContext(dbContext, items);

        IEventRepository repo = new EventRepository(dbContext, redis, NullLogger<EventRepository>.Instance);

        return new EventService(repo, cashe, NullLogger<EventService>.Instance);
    }

    private void PrepareDataDbContext(AppDbContext context)
    {
        context.Events.RemoveRange([.. context.Events]);
        context.SaveChanges();
    }

    private void AddItemsToDbContext<TEntity>(AppDbContext context, List<TEntity>? items) where TEntity : class
    {
        if (items?.Count > 0)
        {
            context.AddRange(items);
            context.SaveChanges();
        }
    }
}