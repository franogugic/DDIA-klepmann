using Microsoft.EntityFrameworkCore;
using MiniProjekt.Models;

namespace MiniProjekt.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; }
}