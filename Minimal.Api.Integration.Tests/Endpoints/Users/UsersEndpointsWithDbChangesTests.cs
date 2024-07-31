using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Minimal.Api.Authentication.Contracts;
using Minimal.Api.Authentication.Endpoints;
using Minimal.Api.Authentication.Features.AuthenticateUser;
using Minimal.Api.Shared.Helpers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AuthProgram = Minimal.Api.Authentication.Program;

namespace Minimal.Api.Integration.Tests.Endpoints.Users;
public class UsersEndpointsWithDbChangesTests : BaseTest, IClassFixture<WebApplicationFactory<AuthProgram>>
{
    private readonly WebApplicationFactory<AuthProgram> _factory;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public UsersEndpointsWithDbChangesTests(WebApplicationFactory<AuthProgram> factory)
    {
        _factory = factory;
        _tokenValidationParameters = _factory.Services.GetRequiredService<TokenValidationParameters>();
    }

    private string BuildResourcePath(string path = "") => $"{UserEndpoints.Resource}/{path}";

    [Fact]
    public async Task RefreshToken_Should_Succed_And_Return_Valid_Token()
    {
        var client = _factory.CreateClient();
        var credentials = new AuthenticateUserRequest { Username = "user2@example.com", Password = "string" };
        var loginContent = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");

        var loginResponse = await client.PostAsync(BuildResourcePath("login"), loginContent);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResponseObject = JsonSerializer.Deserialize<AuthenticateUserResponse>(loginResponseContent, jsonSerializerOptions);

        var refreshTokenContent = new StringContent(JsonSerializer.Serialize(new { loginResponseObject!.RefreshToken }), Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginResponseObject.Token);
        var response = await client.PostAsync(BuildResourcePath("refresh-token"), refreshTokenContent);
        var responseContent = await loginResponse.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<AuthenticateUserResponse>(responseContent, jsonSerializerOptions);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        JwtTokenHelper.IsTokenValid(responseObject!.Token, _tokenValidationParameters).Should().BeTrue();
    }
}
