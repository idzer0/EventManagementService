using EventMS.Events.Domain.Models;
using EventMS.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventMS.Events.Infrastructure.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EventEntity> Events => Set<EventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Использование ApplyConfigurationsFromAssembly мешает созданию миграций, не заполняются методы Up и Down
        modelBuilder.ApplyConfiguration(new EventEntityConfiguration());
    }
}