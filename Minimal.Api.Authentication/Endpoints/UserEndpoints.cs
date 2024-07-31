using FluentValidation;
using MediatR;
using Minimal.Api.Authentication.Contracts;
using Minimal.Api.Authentication.Features;
using Minimal.Api.Authentication.Features.AuthenticateUser;
using Minimal.Api.Authentication.Filters;
using Minimal.Api.Shared;
using Minimal.Api.Shared.Contracts;
namespace Minimal.Api.Authentication.Endpoints;
public static class UserEndpoints
{
    public static string Resource => "users";
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup($"/{Resource}");

        group.MapPost("/", async (CreateUserCommand? request, ISender sender) =>
                await CreateUser(request, sender))
             .WithName("CreateUser")
             .AddEndpointFilter<ValidationFilter<CreateUserCommand>>();

        group.MapPost("/refresh-token", async (HttpContext httpContext, RefreshTokenRequest? request, ISender sender) =>
                await RefreshToken(httpContext, request, sender))
             .WithName("RefreshToken")
             .AddEndpointFilter<ValidationFilter<RefreshTokenCommand>>()
             .RequireAuthorization();

        group.MapPost("/login", async (AuthenticateUserRequest? request, ISender sender, HttpContext httpContext) =>
                await AuthenticateUser(httpContext, request, sender))
             .WithName("AuthenticateUser")
             .AddEndpointFilter<ValidationFilter<AuthenticateUserRequest>>();
    }

    private static async Task<IResult> CreateUser(CreateUserCommand? request, ISender sender)
    {
        if (request is null)
            return Results.BadRequest(new ErrorResponse(ApiErrors.EmptyBody));

        var result = await sender.Send(request);

        return result.IsFailure
            ? Results.BadRequest(new ErrorResponse(result.Errors))
            : Results.Created($"/{Resource}/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> AuthenticateUser(HttpContext httpContext, AuthenticateUserRequest? request, ISender sender)
    {
        if (request is null)
            return Results.BadRequest(new ErrorResponse(ApiErrors.EmptyBody));

        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var remoteHost = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";
        var command = new AuthenticateUserCommand(request.Username, request.Password, clientIp, remoteHost);
        
        var result = await sender.Send(command);

        return result.IsFailure
            ? Results.Unauthorized()
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> RefreshToken(HttpContext httpContext, RefreshTokenRequest? request, ISender sender)
    {
        if (request is null)
            return Results.BadRequest(new ErrorResponse(ApiErrors.EmptyBody));

        var jwtToken = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var remoteHost = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";

        var result = await sender.Send(new RefreshTokenCommand(jwtToken, request.RefreshToken, clientIp, remoteHost));

        return result.IsFailure
            ? Results.BadRequest(new ErrorResponse(result.Errors))
            : Results.Ok(result.Value);
    }
}
