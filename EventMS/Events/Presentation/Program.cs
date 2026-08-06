using Application.DiContext;
using Application.Middleware;
using Application.Services;
//using Application.ServicesBackground;
using Presentation.DiContext;
using Presentation.DiContext.Auth;
using Presentation.DiContext.Presentation;
using Infrastructure.DataAccess;
using Infrastructure.DiContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuth(builder.Configuration);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// if (app.Environment.IsDevelopment())
// {
//     // Инициализация данных
//     try
//     {
//         await DbInitializer.InitializeAsync(app.Services);
//     }
//     catch (Exception ex)
//     {
//         var logger = app.Services.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "Ошибка при инициализации данных.");
//         throw;
//     }
// }

app.Run();


