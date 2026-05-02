using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Infrastructure;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<AppDbContext>>();
        await InitializeAsync(dbContext, logger);
    }

    public static async Task InitializeAsync(AppDbContext dbContext, ILogger? logger = null)
    {
        try
        {
            // Тестовое событие
            var ev = new EventEntity {
                Id = Guid.NewGuid(),
                Title = "Тестовое событие",
                Description = "Это событие создано с целью проверки работоспособности сервиса",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };

            // Создаём тестовые события
            await AddItemsToDbContextAsync(dbContext, [ev]);

            logger?.LogInformation("Тестовые данные созданы");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Ошибка создания тестовых данных.");
            throw;
        }
    }

    private static async Task AddItemsToDbContextAsync<TEntity>(AppDbContext context, List<TEntity> items) where TEntity: class
    {
        await context.AddRangeAsync(items, CancellationToken.None);
        await context.SaveChangesAsync();
    }
}