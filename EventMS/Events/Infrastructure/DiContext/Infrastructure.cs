using Application.Contracts;
using Application.Services;
using Infrastructure.DataAccess;
using Infrastructure.DiContext.Redis;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.DiContext;

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