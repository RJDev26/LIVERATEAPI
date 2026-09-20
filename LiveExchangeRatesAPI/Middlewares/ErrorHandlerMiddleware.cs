using LiveExchangeRatesAPI.Models;
using LiveExchangeRatesAPI.Services;

namespace LiveExchangeRatesAPI.Middlewares;

public sealed class ErrorHandlerMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (
            TransientFailureDetector.IsTransient(exception, context.RequestAborted))
        {
            // A timeout is an availability problem, not an unhandled server error.
            // Avoid writing a second response if IIS/the client has already aborted it.
            if (context.RequestAborted.IsCancellationRequested || context.Response.HasStarted)
            {
                throw;
            }

            logger.LogWarning(exception, "A transient timeout reached the API boundary.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers["Retry-After"] = "60";
            await context.Response.WriteAsJsonAsync(
                new LiveRateResponse(false, null, [], "Live rates are temporarily unavailable."),
                context.RequestAborted);
        }
    }
}
