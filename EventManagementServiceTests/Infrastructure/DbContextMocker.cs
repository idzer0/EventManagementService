using Application.Contracts;
using Application.Services;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

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
        AddItemsToDbContext(dbContext, items);
        return new EventRepository(dbContext, NullLogger<EventRepository>.Instance);
    }

    public IBookingRepository ArrangeBookingRepositoryTestCase(AppDbContext dbContext, List<BookingEntity> items)
    {
        AddItemsToDbContext(dbContext, items);

        Mock<ICurrentUserService> currentUserService = new();
        currentUserService.Setup(service => service.UserId).Returns(1);
        currentUserService.Setup(service => service.Role).Returns((int)UsersRole.User);

        return new BookingRepository(
            dbContext,
            currentUserService.Object,
            NullLogger<BookingRepository>.Instance);
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
        IEventRepository eventRepository,
        List<BookingEntity>? bookings)
    {
        AddItemsToDbContext(dbContext, bookings);

        Mock<ICurrentUserService> currentUserService = new();
        currentUserService.Setup(service => service.UserId).Returns(1);
        currentUserService.Setup(service => service.Role).Returns((int)UsersRole.User);

        IBookingRepository repoBookings = new BookingRepository(
            dbContext,
            currentUserService.Object,
            NullLogger<BookingRepository>.Instance);

        return new BookingService(
            repoBookings,
            eventRepository,
            currentUserService.Object,
            NullLogger<BookingService>.Instance);
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