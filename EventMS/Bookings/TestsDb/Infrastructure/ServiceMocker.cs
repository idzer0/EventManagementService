using Application.Contracts;
using Application.Services;
using Auth.Contracts;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using KafkaSettingsShared.Contracts;
using KafkaSettingsShared.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Bookings.Tests.Infrastructure;

public static class ServiceMocker
{
    public static IBookingRepository ArrangeBookingRepositoryTestCase(AppDbContext dbContext, List<BookingEntity> items)
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

    public static IBookingService ArrangeBookingServiceTestCase(
        AppDbContext dbContext,
        ICurrentUserService currentUserService,
        List<BookingEntity>? bookings)
    {
        AddItemsToDbContext(dbContext, bookings);

        IBookingRepository repoBookings = new BookingRepository(
            dbContext,
            currentUserService,
            NullLogger<BookingRepository>.Instance);

        Mock<IEventPublisher> publisher = new();
        var kafkaSettings = new Mock<IOptions<KafkaSettings>>();

        return new BookingService(
            repoBookings,
            currentUserService,
            publisher.Object,
            kafkaSettings.Object,
            NullLogger<BookingService>.Instance);
    }

    private static void AddItemsToDbContext<TEntity>(AppDbContext context, List<TEntity>? items) where TEntity: class
    {
        if (items?.Count > 0)
        {
            context.AddRange(items);
            context.SaveChanges();
        }
    }
}