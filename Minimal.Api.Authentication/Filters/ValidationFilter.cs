using FluentValidation;
using Minimal.Api.Shared.Contracts;
namespace Minimal.Api.Authentication.Filters;
public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is not null)
        {
            var entity = context.Arguments
              .OfType<T>()
              .FirstOrDefault(a => a?.GetType() == typeof(T));

            if (entity is not null)
            {
                var validationResult = await validator.ValidateAsync(entity);
                if (validationResult.IsValid)
                    return await next(context);

                return Results.BadRequest(new ErrorResponse(validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            return Results.BadRequest($"Could not find type {typeof(T).Name}");
        }

        return await next(context);
    }
}