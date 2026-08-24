using Application.DiContext;
using Application.Middleware;
using Application.ServicesBackground;
using Infrastructure.DataAccess;
using Infrastructure.DiContext;
using Microsoft.EntityFrameworkCore;
using Presentation.DiContext.Auth;
using Presentation.DiContext.Logging;
using Presentation.DiContext.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomSerilog();

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

app.MapPrometheusScrapingEndpoint().AllowAnonymous();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


