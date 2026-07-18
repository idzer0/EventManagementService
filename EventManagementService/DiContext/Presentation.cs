using Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace EventManagementService.DiContext.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {

        services.AddOpenApi();

        services.AddEndpointsApiExplorer();

        // Контроллеры
        services.AddControllers();

        // Добавляем автоматическую валидацию и клиентские адаптеры (опционально)
        // services.AddFluentValidationAutoValidation()
        //         .AddFluentValidationClientsideAdapters();

        // Регистрируем все валидаторы из сборки (где находится RequestValidator)
        services.AddValidatorsFromAssemblyContaining<RequestValidator>();

        // Swagger
        services.AddSwaggerGen();

        return services;
    }
}