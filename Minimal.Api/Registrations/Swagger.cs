using Minimal.Api.Extensions;
using Minimal.Api.Factories.Auth;
namespace Minimal.Api.Registrations;
public static class Swagger
{
    public static void RegisterSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        var authType = configuration.GetAuthType();
        var strategy = SwaggerGenFactory.GetStrategy(authType);

        strategy.AddSwaggerGen(services);
    }
}