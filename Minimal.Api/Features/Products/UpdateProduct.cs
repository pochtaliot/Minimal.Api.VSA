using FluentValidation;
using KellermanSoftware.CompareNetObjects;
using Mapster;
using MediatR;
using Minimal.Api.Database;
using Minimal.Api.Database.Entities;
using Minimal.Api.Database.Repositories;
using Minimal.Api.Shared;
namespace Minimal.Api.Features.Products;
public record UpdateProductCommand(int? Id, string Name, decimal Price) : IRequest<Result<bool>>;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<bool>>
{
    private readonly ProductsDbContext _dbContext;

    public UpdateProductHandler(ProductsDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Result<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (await _dbContext.Products.FindAsync(request.Id, cancellationToken) is Product entity)
        {
            if (new CompareLogic().Compare(request.Adapt<Product>(), entity).AreEqual)
                return Result.Ok(false); // No changes needed

            var product = request.Adapt<Product>();
            _dbContext.Entry(entity).CurrentValues.SetValues(product);
            var changesSaved = await _dbContext.SaveChangesAsync(cancellationToken) > 0;

            return Result.Ok(changesSaved);
        }

        return Result.NotFound<bool>();
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(IProductRepository productRepository)
    {
        RuleFor(c => c.Id).GreaterThan(0); 
        RuleFor(c => c.Price)
            .GreaterThanOrEqualTo(0.01M)
            .LessThanOrEqualTo(100_000_000);
        RuleFor(c => c.Name)
            .MinimumLength(2)
            .MaximumLength(150)
            .MustAsync(async (name, cancellationToken) => !await productRepository.NameExistsAsync(name, cancellationToken))
            .WithMessage("'Name' must be unique");
    }
}
