using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Minimal.Api.Authentication.Contracts;
using Minimal.Api.Authentication.Endpoints;
using Minimal.Api.Shared.Helpers;
using System.Text;
using System.Text.Json;
using AuthProgram = Minimal.Api.Authentication.Program;

namespace Minimal.Api.Integration.Tests.Endpoints.Users;
public class UsersEndpointsWithDbRollbackChangesTests : BaseTest, IClassFixture<UsersCustomWebApplicationFactory<AuthProgram>>
{
    private readonly UsersCustomWebApplicationFactory<AuthProgram> _factory;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public UsersEndpointsWithDbRollbackChangesTests(UsersCustomWebApplicationFactory<AuthProgram> factory)
    {
        _factory = factory;
        _tokenValidationParameters = _factory.Services.GetRequiredService<TokenValidationParameters>();
    }

    private string BuildResourcePath(string path = "") => $"{UserEndpoints.Resource}/{path}";

    [Fact]
    public async Task Login_Should_Succed_And_Return_Valid_Token()
    {
        var client = _factory.CreateClient();
        var credentials = new AuthenticateUserRequest { Username = "user@example.com", Password = "string" };
        var content = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(BuildResourcePath("login"), content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<AuthenticateUserResponse>(responseContent, jsonSerializerOptions);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        JwtTokenHelper.IsTokenValid(responseObject!.Token, _tokenValidationParameters).Should().BeTrue();
    }

    [Fact]
    public async Task Login_Should_Fail()
    {
        var client = _factory.CreateClient();
        var credentials = new AuthenticateUserRequest { Username = "user@example.com", Password = "stri" };
        var content = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(BuildResourcePath("login"), content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
