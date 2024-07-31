namespace Minimal.Api.Authentication.Contracts;

public class RefreshTokenResponse
{
    public string Token { get; private set; }
    public string RefershToken { get; private set; }
    public RefreshTokenResponse(string token, string refreshToken) => (Token, RefershToken) = (token, refreshToken);
}
