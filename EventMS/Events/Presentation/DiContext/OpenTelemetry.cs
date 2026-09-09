using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EventMS.Events.Presentation.DiContext.OpenTelemetry;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenTelemetryService(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("eventservice", "0.0.1"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter()) //Значение enpoint для Exporter будет взято из переменной окружения OTEL_EXPORTER_OTLP_ENDPOINT
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        return services;
    }
}