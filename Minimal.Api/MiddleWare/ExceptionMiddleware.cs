using System.Net;
using System.Text.Json;
namespace Minimal.Api.MiddleWare;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger) =>
        (_next, _logger) = (next, logger);

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try { await _next(httpContext); }
        catch (Exception ex) { await HandleExceptionAsync(httpContext, ex); }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Internal Server Error. Check you data or contact the administrators",
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        _logger.LogError("{0}\n{1}", exception.Message, exception.StackTrace);
        return context.Response.WriteAsync(jsonResponse);
    }
}