using Microsoft.EntityFrameworkCore;
namespace Minimal.Api.Database.Repositories;
public interface IProductRepository
{
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken);
}

public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _dbContext;

    public ProductRepository(ProductsDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken) =>
        await _dbContext.Products.AsNoTracking().AnyAsync(p => p.Name == name, cancellationToken);
}
