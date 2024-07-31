using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Minimal.Api.Authentication.Database;
using Minimal.Api.Authentication.Features.AuthenticateUser.Behaviors;
using System.Net;

namespace Minimal.Api.Integration.Tests;
public class UsersCustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            var bruteForceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(BruteForceBehavior<,>));

            if (bruteForceDescriptor != null)
            {
                services.Remove(bruteForceDescriptor);
            }

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UsersTransactionBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BruteForceBehavior<,>));
            services.AddSingleton<IStartupFilter, CustomStartupFilter>();
        });

        builder.UseEnvironment("Development");
    }
}

public class UsersTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly UsersDbContext _dbContext;

    public UsersTransactionBehavior(UsersDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        TResponse response = await next();
        await transaction.RollbackAsync(cancellationToken);
        return response;
    }
}

public class CustomStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<FakeRemoteIpAddressMiddleware>();
            next(app);
        };
    }
}

public class FakeRemoteIpAddressMiddleware
{
    private readonly RequestDelegate next;
    private static IPAddress fakeIpAddress = IPAddress.Parse("127.168.1.32");

    public static void RandomizeIp()
    {
        var random = new Random();
        byte[] bytes = new byte[4];
        random.NextBytes(bytes);
        fakeIpAddress = new IPAddress(bytes);
    }

    public FakeRemoteIpAddressMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        httpContext.Connection.RemoteIpAddress = fakeIpAddress;

        await this.next(httpContext);
    }
}