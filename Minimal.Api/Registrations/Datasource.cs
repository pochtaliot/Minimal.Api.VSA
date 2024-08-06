using Microsoft.EntityFrameworkCore;
using Minimal.Api.Database;
namespace Minimal.Api.Registrations;
public static class Datasource
{
    public static void RegisterDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductsDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }
}
