namespace Minimal.Api.Shared;
public static class ApiErrors
{
    public const string IdMismatch = "Id mismatch";
    public const string EmptyBody = "Request body cannot be empty";
    public const string LoginFailed = "Wrong e-mail or password";
    public const string LoginAttemptsExceeded = "Too many failed login attempts";
    public const string InvalidToken = "Invalid token";
}
