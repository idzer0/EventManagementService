namespace EventManagementService.DiContext.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        
        services.AddOpenApi();

        services.AddEndpointsApiExplorer();

        // Контроллеры
        services.AddControllers();

        // Swagger
        services.AddSwaggerGen();

        return services;
    }
} 