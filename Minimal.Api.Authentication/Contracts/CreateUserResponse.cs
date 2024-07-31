namespace Minimal.Api.Authentication.Contracts;

public class CreateUserResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public IEnumerable<UserContectInfoResponse> ContactInfo { get; set; } = Enumerable.Empty<UserContectInfoResponse>();
}

public class UserContectInfoResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string MobilePhone { get; set; } = "";
}