using EventMS.Bookings.Application.Contracts;
using EventMS.Bookings.Infrastructure.DataAccess;
using EventMS.Bookings.Infrastructure.Repositories;
using EventMS.Bookings.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventMS.Bookings.Infrastructure.DiContext;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .EnableDetailedErrors());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}