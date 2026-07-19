using Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;


namespace EventManagementService.DiContext.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi();

        services.AddEndpointsApiExplorer();

        // Swagger
        services.AddSwagger();

        // Контроллеры
        services.AddControllers();

        // Регистрируем все валидаторы из сборки (где находится RequestValidator)
        services.AddValidatorsFromAssemblyContaining<RequestValidator>();



        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventMS API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme.
                                Enter 'Bearer' [space] and then your token.
                                Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(document => new()
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

    return services;
    }
}