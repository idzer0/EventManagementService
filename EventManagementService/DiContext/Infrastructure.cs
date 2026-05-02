namespace EventManagementService.DiContext.Infrastructure;

using Microsoft.EntityFrameworkCore;
using EventManagementService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseInMemoryDatabase("EventManagementDb")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        return services;
    }
}