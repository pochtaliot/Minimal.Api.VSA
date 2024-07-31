using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minimal.Api.Contracts.Products;
using Minimal.Api.Endpoints;
using Minimal.Api.Extensions;
using Minimal.Api.Features.Products;
using Minimal.Api.Helpers;
using Minimal.Api.Integration.Tests.TestHelpers;
using Minimal.Api.Integration.Tests.TestHelpers.Models;
using Minimal.Api.Shared.Helpers;
using Minimal.Api.Shared.Settings;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Minimal.Api.Integration.Tests.Endpoints.Products;
public class ProductsEndpointsWithDbRollbackChangesTests : BaseTest, IClassFixture<ProductsCustomWebApplicationFactory<Program>>
{
    private string _apiVersion => $"{ApiVersions.CurrentMajor}.{ApiVersions.CurrentMinor}";
    private readonly ProductsCustomWebApplicationFactory<Program> _factory;
    private readonly IConfiguration? _config;
    private readonly string _token;
    private readonly string _apiKey;

    public ProductsEndpointsWithDbRollbackChangesTests(ProductsCustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _config = _factory.Services.GetRequiredService<IConfiguration>();
        var signingKey = _config.GetSigningKey();
        _token = JwtTokenHelper.GenerateJwtToken(signingKey, new JwtParameters { ValidAudience = "http://localhost", ValidIssuer = "http://localhost" }, Array.Empty<Claim>());
        _apiKey = _config.GetApiKey();
    }

    private string BuildResourcePath(string path = "") => $"v{_apiVersion}/{ProductsEndpoints.Resource}/{path}";

    private void AddAuthorizationHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, _token);
        client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
    }

    [Fact]
    public async Task GetProducts_Should_Return_200_EmptyOrValuedArray()
    {
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.GetAsync(BuildResourcePath());
        var content = await result.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<IEnumerable<ProductResponse>>(content, jsonSerializerOptions);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        responseObject.Should().ContainEquivalentOf(new ProductResponse { Id = 1, Name = "Laptop", Price = 999.99m });
    }

    [Fact]
    public async Task GetProductById_Should_Return_200_ValidObject()
    {
        var productId = 1;
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);
        var expectedResult = new ProductResponse { Id = productId, Name = "Laptop", Price = 999.99m };

        var result = await client.GetAsync(BuildResourcePath(productId.ToString()));
        var content = await result.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<ProductResponse>(content, jsonSerializerOptions);
        
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        responseObject.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetProductById_Should_Return_404_NotFound()
    {
        var productId = 4;
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.GetAsync(BuildResourcePath(productId.ToString()));
     
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Should_Return_201_ValidObjectCreated()
    {
        var newProduct = new CreateProductCommand("Dishwasher", 499.99m);
        var content = new StringContent(JsonSerializer.Serialize(newProduct), Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.PostAsync(BuildResourcePath(), content);
        var responseContent = await result.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<ProductResponse>(responseContent, jsonSerializerOptions);
        
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        responseObject.Should().BeEquivalentTo(newProduct);
    }

    [Fact]
    public async Task Create_Should_Return_400_BadRequest_Unique_Name()
    {
        var newProduct = new CreateProductCommand("Smartphone", 499.99m);
        var content = new StringContent(JsonSerializer.Serialize(newProduct), Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.PostAsync(BuildResourcePath(), content);
        var responseContent = await result.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<ErrorResponseModel>(responseContent, jsonSerializerOptions);
        
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseObject!.Errors.First().Should().Be($"'{nameof(CreateProductCommand.Name)}' must be unique");
    }

    [Fact]
    public async Task Create_Should_Return_400_BadRequest_OtherValidationErrors_MinValue()
    {
        var newProduct = new CreateProductCommand("", 0);
        var content = new StringContent(JsonSerializer.Serialize(newProduct), Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.PostAsync(BuildResourcePath(), content);
        var responseContent = await result.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<ErrorResponseModel>(responseContent, jsonSerializerOptions);
        
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseObject!.Errors.Should().Contain($"The length of '{nameof(CreateProductCommand.Name)}' must be at least 2 characters. You entered 0 characters.");
        responseObject!.Errors.Should().Contain($"'{nameof(CreateProductCommand.Price)}' must be greater than or equal to '0,01'.");
    }

    [Fact]
    public async Task Create_Should_Return_400_BadRequest_OtherValidationError_MaxValue()
    {
        int maxLength = 160;
        var newProduct = new CreateProductCommand(new string('a', maxLength), 100_000_001);
        var content = new StringContent(JsonSerializer.Serialize(newProduct), Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.PostAsync(BuildResourcePath(), content);
        var responseContent = await result.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<ErrorResponseModel>(responseContent, jsonSerializerOptions);
        
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        responseObject!.Errors.Should().Contain($"The length of '{nameof(CreateProductCommand.Name)}' must be 150 characters or fewer. You entered {maxLength} characters.");
        responseObject!.Errors.Should().Contain($"'{nameof(CreateProductCommand.Price)}' must be less than or equal to '100000000'.");
    }

    [Fact]
    public async Task Update_Should_Return_ValidationErros()
    {
        var productId = 2;
        var expectedProduct = new UpdateProductCommand(productId, "", 100000001m);
        var content = new StringContent(JsonSerializer.Serialize(expectedProduct), Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.PutAsync(BuildResourcePath(productId.ToString()), content);
        var resultContent = await result.Content.ReadAsStringAsync();
        var resultObject = JsonSerializer.Deserialize<ErrorResponseModel>(resultContent, jsonSerializerOptions);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        resultObject!.Errors.Should().Contain($"The length of '{nameof(UpdateProductCommand.Name)}' must be at least 2 characters. You entered 0 characters.");
        resultObject!.Errors.Should().Contain($"'{nameof(UpdateProductCommand.Price)}' must be less than or equal to '100000000'.");
    }

    [Fact]
    public async Task Update_Should_Return_204_Updated()
    {
        var productId = 2;
        var expectedProduct = new UpdateProductCommand(productId, "Pan", 499.99m);
        var content = new StringContent(JsonSerializer.Serialize(expectedProduct), Encoding.UTF8, "application/json");
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.PutAsync(BuildResourcePath(productId.ToString()), content);

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Should_Return_204_Deleted()
    {
        var productId = 3;
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.DeleteAsync(BuildResourcePath(productId.ToString()));
        var deletedProductResult = await client.GetAsync(BuildResourcePath(productId.ToString()));

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Should_Return_404_NotFound()
    {
        var productId = 4;
        var client = _factory.CreateClient();
        AddAuthorizationHeaders(client);

        var result = await client.DeleteAsync(BuildResourcePath(productId.ToString()));

        result.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
