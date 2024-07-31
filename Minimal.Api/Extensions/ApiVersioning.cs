using Asp.Versioning;
using Minimal.Api.Helpers;
namespace Minimal.Api.Extensions;
public static class ApiVersioning
{
    public static void RegisterApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(ApiVersions.CurrentMajor, ApiVersions.CurrentMinor);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}
