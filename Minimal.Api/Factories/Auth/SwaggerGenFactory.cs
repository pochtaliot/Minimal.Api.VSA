using Microsoft.OpenApi.Models;
using Minimal.Api.OpenApi;
using Minimal.Api.Shared.Enums;
namespace Minimal.Api.Factories.Auth;
public interface ISwaggerGenStrategy
{
    void AddSwaggerGen(IServiceCollection services);
}
public static class SwaggerGenFactory
{
    private static readonly Dictionary<AuthTypeEnum, ISwaggerGenStrategy> Strategies = new()
    {
        { AuthTypeEnum.Bearer, new JwtAuthSwaggerGenStrategy() },
        { AuthTypeEnum.ApiKey, new ApiKeyAuthSwaggerGenStrategy() }
    };

    public static ISwaggerGenStrategy GetStrategy(AuthTypeEnum authType)
    {
        if (Strategies.TryGetValue(authType, out var strategy))
            return strategy;

        return new NoneAuthSwaggerGenStrategy();
    }
}

public class JwtAuthSwaggerGenStrategy : ISwaggerGenStrategy
{
    public void AddSwaggerGen(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "Bearer",
                Description = "Bearer token to access the api"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

            c.OperationFilter<SwaggerDefaultValues>();
        });
    }
}

public class ApiKeyAuthSwaggerGenStrategy : ISwaggerGenStrategy
{
    public void AddSwaggerGen(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "x-api-key",
                Scheme = "ApiKeyScheme",
                Description = "The API Key to access the API"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            },
                            In = ParameterLocation.Header
                        },
                        Array.Empty<string>()
                    }
                });
        });
    }
}

public class NoneAuthSwaggerGenStrategy : ISwaggerGenStrategy
{
    public void AddSwaggerGen(IServiceCollection services) => services.AddSwaggerGen();
}