namespace EventManagementService.DiContext.Application;

using EventManagementService.Services;
using EventManagementService.Contracts;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
} 