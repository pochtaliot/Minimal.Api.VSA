using Microsoft.EntityFrameworkCore;
using Minimal.Api.Authentication.Database;
namespace Minimal.Api.Authentication.Registrations;
public static class Datasource
{
    public static void RegisterDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        //builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("MinimalApiAuthenticationDb"));
    }
}