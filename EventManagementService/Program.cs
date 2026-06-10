using EventManagementService.DiContext.Application;
using EventManagementService.DiContext;
using EventManagementService.DiContext.Presentation;
using EventManagementService.Infrastructure;
using EventManagementService.Infrastructure.DataAccess;
using EventManagementService.Middleware;
using EventManagementService.ServicesBackground;
using Microsoft.EntityFrameworkCore;
using EventManagementService.DiContext.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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


