using Microsoft.IdentityModel.Tokens;
using Minimal.Api.Shared.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Minimal.Api.Shared.Helpers;
public static class JwtTokenHelper
{
    public static string GenerateJwtToken(string secretKey, JwtParameters jwtParameters, Claim[] claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtParameters.ValidIssuer,
            audience: jwtParameters.ValidAudience,
            expires: DateTime.Now.AddMinutes(jwtParameters.ExpiresMinutes),
            signingCredentials: credentials,
            claims: claims ?? Array.Empty<Claim>()
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public static string GenerateJwtToken(string secretKey, string issuer, string audience, JwtParameters jwtParameters, Claim[] claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.Now.AddMinutes(jwtParameters.ExpiresMinutes),
            signingCredentials: credentials,
            claims: claims ?? Array.Empty<Claim>()
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static ClaimsPrincipal? GetPrincipalFromToken(string token, TokenValidationParameters tokenValidationParameters)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtToken = securityToken as JwtSecurityToken;

            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsTokenValid(string token, TokenValidationParameters tokenValidationParameters)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtToken = securityToken as JwtSecurityToken;

            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
