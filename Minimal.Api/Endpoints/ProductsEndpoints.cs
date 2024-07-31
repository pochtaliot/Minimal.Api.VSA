using Asp.Versioning.Builder;
using FluentValidation;
using MediatR;
using Minimal.Api.Features.Products;
using Minimal.Api.Filters;
using Minimal.Api.Helpers;
using Minimal.Api.Registrations;
using Minimal.Api.Shared;
using Minimal.Api.Shared.Contracts;
namespace Minimal.Api.Endpoints;
public static class ProductsEndpoints
{
    public static string Resource => "products";
    public static void MapProductsEndpoints(this IEndpointRouteBuilder routes, ApiVersionSet versionSet, IConfiguration configuration)
    {
        var group = routes.MapGroup($"v{{version:apiVersion}}/{Resource}")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersions.CurrentMajor, ApiVersions.CurrentMinor);

        group.TryRequireEndpointAuthorization(configuration);

        group.MapGet("/", async (ISender sender) =>
                await GetProducts(sender))
             .WithName("GetProducts");

        group.MapGet("/{id:int}", async (int id, ISender sender) =>
                await GetProductAsync(id, sender))
             .WithName("GetProductById");

        group.MapPost("/", async (CreateProductCommand? request, ISender sender) =>
                await CreateProduct(request, sender))
             .WithName("CreateProduct")
             .AddEndpointFilter<ValidationFilter<CreateProductCommand>>();

        group.MapPut("/{id:int}", async (int id, UpdateProductCommand? request, ISender sender) =>
                await UpdateProduct(id, request, sender))
             .WithName("UpdateProduct")
             .AddEndpointFilter<ValidationFilter<UpdateProductCommand>>();

        group.MapDelete("/{id:int}", async (int id, ISender sender) =>
                await DeleteProduct(id, sender))
             .WithName("DeleteProduct");
    }

    public static async Task<IResult> GetProducts(ISender sender) =>
        Results.Ok(await sender.Send(new GetAllProductsQuery()));

    public static async Task<IResult> GetProductAsync(int id, ISender sender)
    {
        var query = new GetProductQuery(id);
        var result = await sender.Send(query);

        return result!.IsNotFound ? Results.NotFound() : Results.Ok(result!.Value);
    }

    public static async Task<IResult> CreateProduct(CreateProductCommand? request, ISender sender)
    {
        if (request is null)
            return Results.BadRequest(new ErrorResponse(ApiErrors.EmptyBody));

        var result = await sender.Send(request);

        return result.IsFailure
            ? Results.BadRequest(new ErrorResponse(result.Errors))
            : Results.Created($"/{Resource}/{result.Value!.Id}", result.Value);
    }

    public static async Task<IResult> UpdateProduct(int id, UpdateProductCommand? request, ISender sender)
    {
        if (request is null)
            return Results.BadRequest(new ErrorResponse(ApiErrors.EmptyBody));

        if (request.Id != id)
            return Results.BadRequest(new ErrorResponse(ApiErrors.IdMismatch));

        var result = await sender.Send(request);

        return result switch
        {
            { IsNotFound: true } => Results.NotFound(),
            { IsFailure: true } => Results.BadRequest(new ErrorResponse(result.Errors)),
            _ => Results.NoContent()
        };
    }

    public static async Task<IResult> DeleteProduct(int id, ISender sender)
    {
        var command = new DeleteProductCommand(id);
        var result = await sender.Send(command);

        return result switch
        {
            { IsNotFound: true } => Results.NotFound(),
            { IsFailure: true } => Results.BadRequest(new ErrorResponse(result.Errors)),
            _ => Results.NoContent()
        };
    }
}
