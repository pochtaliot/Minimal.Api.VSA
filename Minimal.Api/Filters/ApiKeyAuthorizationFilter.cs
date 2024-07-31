
using Minimal.Api.Extensions;
namespace Minimal.Api.Filters;
public class ApiKeyAuthorizationFilter : IEndpointFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthorizationFilter> _logger;

    public ApiKeyAuthorizationFilter(IConfiguration configuration, ILogger<ApiKeyAuthorizationFilter> logger) =>
        (_configuration, _logger) = (configuration, logger);

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("x-api-key", out var extractedApiKey))
        {
            _logger.LogWarning("Api key is missing");
            return TypedResults.Unauthorized();
        }

        var apiKey = _configuration.GetApiKey();
        if (apiKey != extractedApiKey)
        {
            _logger.LogWarning("Api key is invalid");
            return TypedResults.Unauthorized();
        }

        return await next(context);
    }
}
