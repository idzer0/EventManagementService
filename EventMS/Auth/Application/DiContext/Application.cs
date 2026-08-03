using Application.Contracts;
using Application.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Application.DiContext;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}