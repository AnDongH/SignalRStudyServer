namespace SignalRStudyServer.Middlewares;

public class PathLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PathLoggingMiddleware> _logger;
    
    public PathLoggingMiddleware(RequestDelegate next, ILogger<PathLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        _logger.LogInformation(context.Request.Method + " " + context.Request.Path);
        await _next(context);

    }
    
}

public static class PathLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UsePathLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PathLoggingMiddleware>();
    }
}