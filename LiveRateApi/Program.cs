using LiveRateApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LiveRateCache>();
builder.Services.AddHttpClient<LiveRateService>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.Timeout = TimeSpan.FromSeconds(
        configuration.GetValue("LiveRates:TimeoutSeconds", 10));
});

var app = builder.Build();

app.MapGet("/api/liverates", async (LiveRateService service, CancellationToken cancellationToken) =>
{
    var result = await service.GetRatesAsync(cancellationToken);
    return result.IsSuccess
        ? Results.Ok(result)
        : Results.Json(result, statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();

public partial class Program;
