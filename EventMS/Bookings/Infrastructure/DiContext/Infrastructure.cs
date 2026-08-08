using Application.Contracts;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DiContext;

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