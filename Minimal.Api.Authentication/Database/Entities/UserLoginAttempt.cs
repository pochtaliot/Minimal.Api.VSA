namespace Minimal.Api.Authentication.Database.Entities;
public class UserLoginAttempt
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string? IpAddress { get; set; } = "";
    public int AttemptsCount { get; set; } = 1;
    public DateTime Modified { get; set; }
}
