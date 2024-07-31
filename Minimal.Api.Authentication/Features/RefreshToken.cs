using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minimal.Api.Authentication.Contracts;
using Minimal.Api.Authentication.Database;
using Minimal.Api.Shared;
using Minimal.Api.Shared.Extensions;
using Minimal.Api.Shared.Helpers;
using Minimal.Api.Shared.Settings;
using System.Security.Claims;
namespace Minimal.Api.Authentication.Features;
public record RefreshTokenCommand(string Token, string RefreshToken, string? ClientIp, string RemoteHost) : IRequest<Result<RefreshTokenResponse>>;
public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly UsersDbContext _dbContext;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtParameters _jwtParameters;

    public RefreshTokenHandler(UsersDbContext dbContext, TokenValidationParameters tokenValidationParameters, JwtParameters jwtParameters) =>
        (_dbContext, _tokenValidationParameters, _jwtParameters) = (dbContext, tokenValidationParameters, jwtParameters);

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = JwtTokenHelper.GetPrincipalFromToken(request.Token, _tokenValidationParameters);

        if (principal == null)
            return Result.Fail<RefreshTokenResponse>("Invalid token");

        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(name))
            return Result.Fail<RefreshTokenResponse>("Invalid token payload");

        var user = await _dbContext.Users.Include(i => i.RefreshToken).FirstOrDefaultAsync(u => u.Email == name);
        if (user is null)
            return Result.Fail<RefreshTokenResponse>("Token doesn't contain existing username");

        if (user.RefreshToken!.Token != request.RefreshToken)
            return Result.Fail<RefreshTokenResponse>("Invalid refresh token");

        var claims = new Claim[]
        {
            new Claim(ClaimTypes.Name, user.Email)
        };

        string token = JwtTokenHelper.GenerateJwtToken(
            _jwtParameters.SigningKey,
            request.RemoteHost,
            request.RemoteHost,
            _jwtParameters,
            claims);

        string refreshToken = StringExtension.GenerateRandomString(_jwtParameters.RefreshTokenLength);

        await _dbContext.UsersRefreshTokens.Where(w => w.UserId == user.Id)
            .ExecuteUpdateAsync(u => u.SetProperty(ut => ut.Token, refreshToken), cancellationToken);

        RefreshTokenResponse refreshTokenResponse = new RefreshTokenResponse(token, refreshToken);

        return Result.Ok(refreshTokenResponse);
    }
}