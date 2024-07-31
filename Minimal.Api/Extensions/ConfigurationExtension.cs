using Minimal.Api.Shared.Enums;
namespace Minimal.Api.Extensions;
public static class ConfigurationExtension
{
    public static bool BearerAuthenticationEnabled(this IConfiguration configuration) =>
        configuration.GetAuthType() == AuthTypeEnum.Bearer;
    public static bool ApiKeyAuthenticationEnabled(this IConfiguration configuration) =>
        configuration.GetAuthType() == AuthTypeEnum.ApiKey;
    public static IEnumerable<string> GetValidIssuers(this IConfiguration configuration) =>
        configuration.GetSection("Authentication:Schemes:Bearer:ValidIssuers").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();
    public static IEnumerable<string> GetValidAudiences(this IConfiguration configuration) =>
        configuration.GetSection("Authentication:Schemes:Bearer:ValidAudiences").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();
    public static string GetSigningKey(this IConfiguration configuration) =>
        configuration.GetValue<string>("Authentication:Schemes:Bearer:SigningKey") ?? string.Empty;
    public static AuthTypeEnum GetAuthType(this IConfiguration configuration) =>
        Enum.TryParse(configuration.GetValue<string>("Authentication:Type"), out AuthTypeEnum authType)
        ? authType
        : AuthTypeEnum.None;
    public static string GetApiKey(this IConfiguration configuration) =>
        configuration.GetValue<string>("Authentication:Schemes:ApiKey:Key") ?? string.Empty;
}
