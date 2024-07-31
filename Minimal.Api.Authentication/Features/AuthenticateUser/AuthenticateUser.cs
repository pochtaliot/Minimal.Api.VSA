using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.Authentication.Contracts;
using Minimal.Api.Authentication.Database;
using Minimal.Api.Authentication.Extensions;
using Minimal.Api.Shared;
using Minimal.Api.Shared.Extensions;
using Minimal.Api.Shared.Helpers;
using Minimal.Api.Shared.Settings;
using System.Security.Claims;
namespace Minimal.Api.Authentication.Features.AuthenticateUser;
public record AuthenticateUserCommand(string Username, string Password, string? ClientIp, string RemoteHost) : IRequest<Result<AuthenticateUserResponse>>;
public class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, Result<AuthenticateUserResponse>>
{
    private readonly UsersDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly JwtParameters _jwtParameters;
    public AuthenticateUserHandler(
          UsersDbContext dbContext
        , IConfiguration configuration
        , JwtParameters jwtParameters
        ) => (_dbContext, _configuration, _jwtParameters) = (dbContext, configuration, jwtParameters);

    public async Task<Result<AuthenticateUserResponse>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Username && u.Active, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
            return Result.Fail<AuthenticateUserResponse>(ApiErrors.LoginFailed);

        var claims = new Claim[]
        {
            new Claim(ClaimTypes.Name, request.Username)
        };

        string token = JwtTokenHelper.GenerateJwtToken(
            _configuration.GetSigningKey(),
            request.RemoteHost,
            request.RemoteHost,
            _jwtParameters,
            claims);

        string refreshToken = StringExtension.GenerateRandomString(_jwtParameters.RefreshTokenLength);

        await _dbContext.UsersRefreshTokens.Where(w => w.UserId == user.Id)
            .ExecuteUpdateAsync(u => u.SetProperty(ut => ut.Token, refreshToken), cancellationToken);

        return Result.Ok(new AuthenticateUserResponse { Token = token, RefreshToken = refreshToken });
    }
}

public class AuthenticateUserValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserValidator() =>
        RuleFor(c => c.Username).EmailAddress();
}