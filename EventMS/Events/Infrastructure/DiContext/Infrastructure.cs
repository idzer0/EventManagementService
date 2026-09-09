using EventMS.Events.Application.Contracts;
using EventMS.Events.Application.Services;
using EventMS.Events.Infrastructure.DataAccess;
using EventMS.Events.Infrastructure.DiContext.Redis;
using EventMS.Events.Infrastructure.Repositories;
using EventMS.Events.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EventMS.Events.Infrastructure.DiContext;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .EnableDetailedErrors());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();

        services.AddRedis(configuration);

        return services;
    }
}