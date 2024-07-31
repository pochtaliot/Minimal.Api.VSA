using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Minimal.Api.Extensions;
using Minimal.Api.Filters;
using Minimal.Api.Shared.Enums;
using System.Text;
namespace Minimal.Api.Factories.Auth;
public interface IAuthStrategy
{
    void Register(IServiceCollection services, IConfiguration configuration);
    void RegisterForEndpoints(RouteGroupBuilder endpointGroup);
}

public static class AuthStrategyFactory
{
    private static readonly Dictionary<AuthTypeEnum, IAuthStrategy> Strategies = new()
    {
        { AuthTypeEnum.Bearer, new JwtAuthStrategy() },
        { AuthTypeEnum.ApiKey, new ApiKeyAuthStrategy() }
    };

    public static IAuthStrategy GetStrategy(AuthTypeEnum authType)
    {
        if (Strategies.TryGetValue(authType, out var strategy))
            return strategy;
        
        return new NoneAuthStrategy();
    }
}

public class JwtAuthStrategy : IAuthStrategy
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuers = configuration.GetValidIssuers(),
                    ValidAudiences = configuration.GetValidAudiences(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSigningKey())),
                };
            });

        services.AddAuthorization();
    }

    public void RegisterForEndpoints(RouteGroupBuilder endpointGroup) =>
        endpointGroup.RequireAuthorization();
}

public class ApiKeyAuthStrategy : IAuthStrategy
{
    public void Register(IServiceCollection services, IConfiguration configuration) { }

    public void RegisterForEndpoints(RouteGroupBuilder endpointGroup) =>
        endpointGroup.AddEndpointFilter<ApiKeyAuthorizationFilter>();
}

public class NoneAuthStrategy : IAuthStrategy
{
    public void Register(IServiceCollection services, IConfiguration configuration) { }
    public void RegisterForEndpoints(RouteGroupBuilder endpointGroup) { }
}