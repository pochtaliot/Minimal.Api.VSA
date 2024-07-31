using Minimal.Api.Extensions;
using Minimal.Api.Factories.Auth;
namespace Minimal.Api.Registrations;
public static class Auth
{
    public static void RegisterAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var authType = configuration.GetAuthType();
        var strategy = AuthStrategyFactory.GetStrategy(authType);

        strategy.Register(services, configuration);
    }

    public static void TryRequireEndpointAuthorization(this RouteGroupBuilder endpointGroup, IConfiguration configuration)
    {
        var authType = configuration.GetAuthType();
        var strategy = AuthStrategyFactory.GetStrategy(authType);

        strategy.RegisterForEndpoints(endpointGroup);
    }
}