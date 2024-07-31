namespace Minimal.Api.Authentication.Database.Entities;

public class UserRefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = "";
    public User? User { get; set; }
}