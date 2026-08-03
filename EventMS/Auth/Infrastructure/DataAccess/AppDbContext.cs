using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Использование ApplyConfigurationsFromAssembly мешает созданию миграций, не заполняются методы Up и Down
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
    }
}