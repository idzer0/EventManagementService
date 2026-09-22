using EventMS.Bookings.Application.Contracts;
using EventMS.Bookings.Application.Services;
using EventMS.Auth.Contracts;
using EventMS.Bookings.Domain.Models;
using EventMS.Bookings.Infrastructure.DataAccess;
using EventMS.Bookings.Infrastructure.Repositories;
using KafkaSettingsShared.Contracts;
using KafkaSettingsShared.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventMS.Bookings.Tests.Infrastructure;

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

    public IBookingRepository ArrangeBookingRepositoryTestCase(AppDbContext dbContext, List<BookingEntity> items)
    {
        AddItemsToDbContext(dbContext, items);

        Mock<ICurrentUserService> currentUserService = new();
        currentUserService.Setup(service => service.UserId).Returns(1);
        currentUserService.Setup(service => service.Role).Returns(UsersRole.User);
        currentUserService.Setup(service => service.IsAllowUserOperation(1)).Returns(true);

        return new BookingRepository(
            dbContext,
            currentUserService.Object,
            NullLogger<BookingRepository>.Instance);
    }

    public IBookingService ArrangeBookingServiceTestCase(
        AppDbContext dbContext,
        List<BookingEntity>? bookings)
    {
        AddItemsToDbContext(dbContext, bookings);

        Mock<ICurrentUserService> currentUserService = new();
        currentUserService.Setup(service => service.UserId).Returns(1);
        currentUserService.Setup(service => service.Role).Returns(UsersRole.User);
        currentUserService.Setup(service => service.IsAllowUserOperation(1)).Returns(true);

        IBookingRepository repoBookings = new BookingRepository(
            dbContext,
            currentUserService.Object,
            NullLogger<BookingRepository>.Instance);

        Mock<IEventPublisher> publisher = new();
        var kafkaSettings = new KafkaSettings();
        var options = Options.Create(kafkaSettings);

        publisher.Setup(p => p.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);


        return new BookingService(
            repoBookings,
            currentUserService.Object,
            publisher.Object,
            options,
            NullLogger<BookingService>.Instance);
    }


    private void PrepareDataDbContext(AppDbContext context)
    {
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