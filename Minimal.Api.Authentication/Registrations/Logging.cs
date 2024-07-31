using Serilog;
namespace Minimal.Api.Authentication.Registrations;
public static class Logging
{
    public static void AddCustomLogging(this IServiceCollection services) =>
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Console()
            .CreateLogger();
}