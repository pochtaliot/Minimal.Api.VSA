namespace Minimal.Api.Authentication.Contracts;

public class AuthenticateUserRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
