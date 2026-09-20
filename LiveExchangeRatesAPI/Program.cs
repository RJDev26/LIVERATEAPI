using LiveExchangeRatesAPI.Middlewares;
using LiveExchangeRatesAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LiveRateCache>();
builder.Services.AddOutputCache();
builder.Services.AddHttpClient<LiveRateService>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.Timeout = TimeSpan.FromSeconds(
        configuration.GetValue("LiveRates:TimeoutSeconds", 10));
});

var app = builder.Build();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseOutputCache();

app.MapGet("/", () => Results.Ok(new
{
    IsSuccess = true,
    Service = "Live Rate API",
    LiveRatesEndpoint = "/api/liverates"
}));

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Application = typeof(Program).Assembly.GetName().Name
}));

app.MapGet("/api/liverates", async (LiveRateService service, CancellationToken cancellationToken) =>
{
    var result = await service.GetRatesAsync(cancellationToken);
    return result.IsSuccess
        ? Results.Ok(result)
        : Results.Json(result, statusCode: StatusCodes.Status503ServiceUnavailable);
})
.CacheOutput(policy => policy
    .Expire(TimeSpan.FromMinutes(1))
    .SetLocking(true));

app.Run();

public partial class Program;
