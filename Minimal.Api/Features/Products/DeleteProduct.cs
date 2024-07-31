using FluentValidation;
using MediatR;
using Minimal.Api.Database;
using Minimal.Api.Shared;
namespace Minimal.Api.Features.Products;
public record DeleteProductCommand(int Id) : IRequest<Result<bool>>;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly ProductsDbContext _dbContext;

    public DeleteProductHandler(ProductsDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Result<bool>> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.Products.FindAsync(command.Id, cancellationToken);

        if (entry is not null)
        {
            _dbContext.Products.Remove(entry);
            var deleteResult = await _dbContext.SaveChangesAsync(cancellationToken) > 0;
            return Result.Ok(deleteResult);
        }

        return Result.NotFound<bool>();
    }
}

public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator() => RuleFor(c => c.Id).GreaterThan(0);
}