using EventMS.Auth.Application.Contracts;
using EventMS.Auth.Application.Services;

using Microsoft.Extensions.DependencyInjection;

namespace EventMS.Auth.Application.DiContext;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}