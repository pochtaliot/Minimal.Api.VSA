using FluentValidation;
using MediatR;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Minimal.Api.Authentication.Endpoints;
using Minimal.Api.Authentication.Features.AuthenticateUser.Behaviors;
using Minimal.Api.Authentication.Middleware;
using Minimal.Api.Authentication.Registrations;
using Minimal.Api.Database;
using Minimal.Api.Shared.Settings;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomLogging();
builder.Services.RegisterDb(builder.Configuration);
builder.Services.RegisterAuthorization(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterSwagger();
builder.Services.AddSingleton<JwtParameters>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddFluentValidationRulesToSwagger();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BruteForceBehavior<,>));
builder.Host.UseSerilog();

var app = builder.Build();

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        return Task.CompletedTask;
    });

    await next.Invoke();
});

if (app.Environment.IsDevelopment())
{
    app.Services.InitializeDatabase();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapUserEndpoints();
app.Services.InitializeParameters();

await app.RunAsync();

namespace Minimal.Api.Authentication
{
    public partial class Program { }
}