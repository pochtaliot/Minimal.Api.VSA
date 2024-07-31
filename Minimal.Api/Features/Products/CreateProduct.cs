using FluentValidation;
using Mapster;
using MediatR;
using Minimal.Api.Contracts.Products;
using Minimal.Api.Database;
using Minimal.Api.Database.Entities;
using Minimal.Api.Database.Repositories;
using Minimal.Api.Shared;
namespace Minimal.Api.Features.Products;
public record CreateProductCommand(string Name, decimal Price) : IRequest<Result<ProductResponse>>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
{
    private readonly ProductsDbContext _dbContext;

    public CreateProductHandler(ProductsDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = request.Adapt<Product>();
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        return Result.Ok(product.Adapt<ProductResponse>());
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IProductRepository productRepository)
    {
        RuleFor(c => c.Price).GreaterThanOrEqualTo(0.01M).LessThanOrEqualTo(100_000_000);
        RuleFor(c => c.Name)
            .MinimumLength(2)
            .MaximumLength(150)
            .MustAsync(async (name, cancellationToken) => !await productRepository.NameExistsAsync(name, cancellationToken))
            .WithMessage($"'{nameof(CreateProductCommand.Name)}' must be unique");
    }
}