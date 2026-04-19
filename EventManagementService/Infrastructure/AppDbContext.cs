using Microsoft.EntityFrameworkCore;
using EventManagementService.Models;

namespace EventManagementService.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EventEntity> Events { get; set; }
}