using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.Contracts.Products;
using Minimal.Api.Database;
namespace Minimal.Api.Features.Products;
public class GetAllProductsQuery() : IRequest<IEnumerable<ProductResponse>>;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductResponse>>
{
    private readonly ProductsDbContext _dbContext;

    public GetAllProductsHandler(ProductsDbContext dbContext) => 
        _dbContext = dbContext;

    public async Task<IEnumerable<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken) => 
        (await _dbContext.Products.Take(50).AsNoTracking().ToListAsync(cancellationToken)).Adapt<IEnumerable<ProductResponse>>();
}