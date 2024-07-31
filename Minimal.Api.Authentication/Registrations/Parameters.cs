using Minimal.Api.Authentication.Extensions;
using Minimal.Api.Shared.Settings;
namespace Minimal.Api.Authentication.Registrations;
public static class Parameters
{
    public static void InitializeParameters(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var jwtParameters = scope.ServiceProvider.GetRequiredService<JwtParameters>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        jwtParameters.ExpiresMinutes = config.GetTokenExpirationMinutes();
        jwtParameters.ValidIssuer = config.GetValidIssuer();
        jwtParameters.ValidAudience = config.GetValidAudience();
        jwtParameters.RefreshTokenLength = config.GetRefreshTokenLength();
        jwtParameters.RefreshTokenExpirationHours = config.GetRefreshTokenExpirationHours();
        jwtParameters.SigningKey = config.GetSigningKey();
    }
}
