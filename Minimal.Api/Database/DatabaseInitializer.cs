using Minimal.Api.Database.Entities;
namespace Minimal.Api.Database;
public static class DatabaseInitializer
{
    public static void InitializeDatabase(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
            context.Database.EnsureCreated();

            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { Name = "Laptop", Price = 999.99m },
                    new Product { Name = "Smartphone", Price = 499.99m },
                    new Product { Name = "Tablet", Price = 299.99m }
                );
                context.SaveChanges();
            }
        }
    }
}