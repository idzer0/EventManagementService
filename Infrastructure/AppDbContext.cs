namespace EventManagementService.Infrastructure;

using Microsoft.EntityFrameworkCore;
using EventManagementService.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EventEntity> Events { get; set; }
}