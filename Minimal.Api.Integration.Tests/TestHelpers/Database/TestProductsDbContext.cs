using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Minimal.Api.Database;

namespace Minimal.Api.Integration.Tests.TestHelpers.Database;
public class TestProductsDbContext : ProductsDbContext
{
    public TestProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using IDbContextTransaction transaction = await Database.BeginTransactionAsync();
        return 0;
    }
}
