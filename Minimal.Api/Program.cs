using Asp.Versioning;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Minimal.Api.Database;
using Minimal.Api.Endpoints;
using Minimal.Api.Extensions;
using Minimal.Api.Helpers;
using Minimal.Api.MiddleWare;
using Minimal.Api.OpenApi;
using Minimal.Api.Registrations;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomLogging();
builder.Services.RegisterDb(builder.Configuration);
builder.Services.RegisterApiVersioning();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.RegisterAuthorization(builder.Configuration);
builder.Services.RegisterSwagger(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddFluentValidationRulesToSwagger();
builder.Host.UseSerilog();

var app = builder.Build();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.Services.InitializeDatabase();
    app.UseSwagger();
    app.UseSwaggerUI(
    options =>
    {
        var descriptions = app.DescribeApiVersions();

        foreach (var description in descriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName);
        }
    });
}

if (builder.Configuration.BearerAuthenticationEnabled())
{
    app.UseAuthentication();
    app.UseAuthorization(); 
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(ApiVersions.CurrentMajor, ApiVersions.CurrentMinor))
    .Build();

app.MapProductsEndpoints(versionSet, builder.Configuration);

await app.RunAsync();

namespace Minimal.Api
{
    public partial class Program { }
}