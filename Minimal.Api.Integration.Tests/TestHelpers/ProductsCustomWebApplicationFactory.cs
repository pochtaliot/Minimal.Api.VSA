using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Minimal.Api.Database;

namespace Minimal.Api.Integration.Tests.TestHelpers;
public class ProductsCustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ProductsTransactionBehavior<,>));
        });

        builder.UseEnvironment("Development");
    }
}

public class ProductsTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ProductsDbContext _dbContext;

    public ProductsTransactionBehavior(ProductsDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        TResponse response = await next();
        await transaction.RollbackAsync(cancellationToken);
        return response;
    }
}