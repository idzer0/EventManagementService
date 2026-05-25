using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Infrastructure.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Использование ApplyConfigurationsFromAssembly мешает созданию миграций, не заполняются методы Up и Down
        modelBuilder.ApplyConfiguration(new BookingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EventEntityConfiguration());
    }
}