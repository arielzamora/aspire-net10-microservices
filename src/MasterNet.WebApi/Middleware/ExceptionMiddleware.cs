using MasterNet.Application.Core;
using Newtonsoft.Json;

namespace MasterNet.WebApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env
    )
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the exception middleware will not modify the response.");
                throw;
            }

            _logger.LogError(ex, ex.Message);

            var traceId = context.TraceIdentifier;

            // Since validation is already handled by the ValidationBehavior (returning Result<T>.Failure)
            // and/or ApiBehaviorOptions.InvalidModelStateResponseFactory, we do NOT special-case ValidationException here.
            // This middleware is now focused exclusively on unexpected errors.
            var statusCode = StatusCodes.Status500InternalServerError;
            var title = "An unexpected error occurred.";
            var detail = _env.IsDevelopment()
                ? $"{ex.Message}{Environment.NewLine}{ex.StackTrace}"
                : null;

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = new
            {
                status = statusCode,
                title,
                detail,
                traceId
            };

            var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await context.Response.WriteAsync(json);
        }
    }
}


