namespace Minimal.Api.Shared.Settings;
public class JwtParameters
{
    public string ValidAudience { get; set; } = "";
    public string ValidIssuer { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 30;
    public int RefreshTokenLength { get; set; } = 60;
    public int RefreshTokenExpirationHours { get; set; } = 12;
    public string SigningKey { get; set; } = "";
}
