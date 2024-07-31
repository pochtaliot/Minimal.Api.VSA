using Minimal.Api.Database.Repositories;
namespace Minimal.Api.Registrations;
public static class Repositories
{
    public static void AddRepositories(this IServiceCollection services) =>
        services.AddScoped<IProductRepository, ProductRepository>();
}
