using EventManagementService.Contracts;
using EventManagementService.Infrastructure;
using EventManagementService.Models;
using EventManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTests.Infrastructure;

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

    public IEventRepository ArrangeEventsRepositoryTestCase(AppDbContext dbContext, List<EventEntity> items)
    {
        dbContext.Events.AddRange(items);
        dbContext.SaveChanges();

        return new EventRepository(dbContext, NullLogger<EventRepository>.Instance);
    }

    public IBookingRepository ArrangeBookingRepositoryTestCase(AppDbContext dbContext, List<BookingEntity> items)
    {
        dbContext.Bookings.AddRange(items);
        dbContext.SaveChanges();

        return new BookingRepository(dbContext, NullLogger<BookingRepository>.Instance);
    }

    public IEventService ArrangeEventServiceTestCase(
        AppDbContext dbContext,
        List<EventEntity>? items)
    {
        AddItemsToDbContext(dbContext, items);

        IEventRepository repo =  new EventRepository(dbContext, NullLogger<EventRepository>.Instance);

        return new EventService(repo, NullLogger<EventService>.Instance);
    }

    public IBookingService ArrangeBookingServiceTestCase(
        AppDbContext dbContext,
        IEventService eventService,
        List<BookingEntity>? bookings)
    {
        AddItemsToDbContext(dbContext, bookings);

        IBookingRepository repoBookings =  new BookingRepository(dbContext, NullLogger<BookingRepository>.Instance);

        return new BookingService(repoBookings, eventService, NullLogger<BookingService>.Instance);
    }


    private void PrepareDataDbContext(AppDbContext context)
    {
        context.Events.RemoveRange([.. context.Events]);
        context.Bookings.RemoveRange([.. context.Bookings]);
        context.SaveChanges();
    }

    private void AddItemsToDbContext<TEntity>(AppDbContext context, List<TEntity>? items) where TEntity: class
    {
        if (items?.Count > 0)
        {
            context.AddRange(items);
            context.SaveChanges();
        }
    }
}