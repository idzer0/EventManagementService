using EventManagementService.Infrastructure.DataAccess;
namespace EventManagementService.DiContext.Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .EnableDetailedErrors());

        return services;
    }
}