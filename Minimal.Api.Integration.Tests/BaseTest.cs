using System.Text.Json;

namespace Minimal.Api.Integration.Tests;
public abstract class BaseTest
{
    protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
}
