namespace Minimal.Api.Authentication.Contracts;

public class AuthenticateUserResponse
{
    public string Token { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}