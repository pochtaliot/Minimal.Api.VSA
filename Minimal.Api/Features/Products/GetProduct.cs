using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.Contracts.Products;
using Minimal.Api.Database;
using Minimal.Api.Shared;
namespace Minimal.Api.Features.Products;
public record GetProductQuery(int Id) : IRequest<Result<ProductResponse>?>;

public class GetProductHandler : IRequestHandler<GetProductQuery, Result<ProductResponse>?>
{
    private readonly ProductsDbContext _dbContext;

    public GetProductHandler(ProductsDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Result<ProductResponse>?> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.Products.FindAsync(query.Id, cancellationToken);

        return entry switch
        {
            not null => Result.Ok(entry.Adapt<ProductResponse>()),
                   _ => Result.NotFound<ProductResponse>()
        };
    }
}