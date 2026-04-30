using EventManagementService.DiContext.Application;
using EventManagementService.DiContext.Infrastructure;
using EventManagementService.DiContext.Presentation;
using EventManagementService.Infrastructure;
using EventManagementService.Middleware;
using EventManagementService.ServicesBackground;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddPresentation();

// Включаем валидацию только в Development
if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

builder.Services.AddHostedService<BookingBackgroundProcessing>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // Инициализация данных
    try
    {
        await DbInitializer.InitializeAsync(app.Services);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации данных.");
        throw;
    }
}

app.Run();


