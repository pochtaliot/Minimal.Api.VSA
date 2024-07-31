namespace Minimal.Api.Authentication.Extensions;
public static class ConfigurationExtension
{
    public static string GetValidIssuer(this IConfiguration configuration) =>
        configuration.GetValue<string>("Authentication:Schemes:Bearer:ValidIssuer") ?? string.Empty;
    public static string GetValidAudience(this IConfiguration configuration) =>
        configuration.GetValue<string>("Authentication:Schemes:Bearer:ValidAudience") ?? string.Empty;
    public static string GetSigningKey(this IConfiguration configuration) =>
        configuration.GetValue<string>("Authentication:Schemes:Bearer:SigningKey") ?? string.Empty;
    public static int GetTokenExpirationMinutes(this IConfiguration configuration) =>
        configuration.GetValue<int?>("Authentication:Schemes:Bearer:TokenExpirationMinutes") ?? 30;
    public static int GetRefreshTokenLength(this IConfiguration configuration) =>
        configuration.GetValue<int?>("Authentication:Schemes:Bearer:RefreshTokenLength") ?? 60;
    public static int GetRefreshTokenExpirationHours(this IConfiguration configuration) =>
        configuration.GetValue<int?>("Authentication:Schemes:Bearer:RefreshTokenExpirationHours") ?? 12; 
    public static IEnumerable<string> GetValidIssuers(this IConfiguration configuration) =>
        configuration.GetSection("Authentication:Schemes:Bearer:ValidIssuers").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();
    public static IEnumerable<string> GetValidAudiences(this IConfiguration configuration) =>
        configuration.GetSection("Authentication:Schemes:Bearer:ValidAudiences").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();
    public static int GetLoginAttemptsLimit(this IConfiguration configuration) =>
        configuration.GetValue<int?>("Authentication:LoginAttemptsLimit") ?? 10;
}