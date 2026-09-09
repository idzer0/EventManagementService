using EventMS.Bookings.Domain.Models;
using EventMS.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventMS.Bookings.Infrastructure.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Использование ApplyConfigurationsFromAssembly мешает созданию миграций, не заполняются методы Up и Down
        modelBuilder.ApplyConfiguration(new BookingEntityConfiguration());
    }
}