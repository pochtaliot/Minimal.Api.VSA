using Minimal.Api.Authentication.Database;
using Minimal.Api.Authentication.Database.Entities;
using Minimal.Api.Shared.Extensions;
using Minimal.Api.Shared.Settings;
namespace Minimal.Api.Database;
public static class DatabaseInitializer
{
    public static void InitializeDatabase(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            var jwtParameters = scope.ServiceProvider.GetRequiredService<JwtParameters>();
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Email = "user@example.com",
                        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("string"),
                        RefreshToken = new UserRefreshToken
                        {
                            Token = StringExtension.GenerateRandomString(jwtParameters.RefreshTokenLength)
                        },
                        Active = true
                    },
                    new User
                    {
                        Email = "user2@example.com",
                        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("string"),
                        RefreshToken = new UserRefreshToken
                        {
                            Token = StringExtension.GenerateRandomString(jwtParameters.RefreshTokenLength)
                        },
                        Active = true
                    }
                );

                context.SaveChanges();
            }
        }
    }
}