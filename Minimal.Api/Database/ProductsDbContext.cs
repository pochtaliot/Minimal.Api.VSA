using Microsoft.EntityFrameworkCore;
using Minimal.Api.Database.Entities;
namespace Minimal.Api.Database;
public class ProductsDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Name)
                  .IsUnique();

            entity.Property(e => e.Price)
                  .HasPrecision(18, 2);
        });
    }
}
