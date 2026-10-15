using AiBusinessAssistant.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AiBusinessAssistant.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException exception)
        {
            var status = exception switch
            {
                ApiValidationException => StatusCodes.Status400BadRequest,
                ApiNotFoundException => StatusCodes.Status404NotFound,
                ApiConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = status switch
                {
                    StatusCodes.Status400BadRequest => "Invalid request",
                    StatusCodes.Status404NotFound => "Resource not found",
                    StatusCodes.Status409Conflict => "Request conflict",
                    _ => "Request failed"
                },
                Detail = exception.Message,
                Instance = context.Request.Path
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            if (context.Response.HasStarted) throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Instance = context.Request.Path
            });
        }
    }
}
