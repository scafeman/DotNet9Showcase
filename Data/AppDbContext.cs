using Microsoft.EntityFrameworkCore;
using DotNet9Showcase.Models;

namespace DotNet9Showcase.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
}
