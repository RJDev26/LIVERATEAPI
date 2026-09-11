using System.Net.Http.Json;
using LiveRateApi.Models;

namespace LiveRateApi.Services;

public sealed class LiveRateService(
    HttpClient httpClient,
    IConfiguration configuration,
    LiveRateCache cache,
    ILogger<LiveRateService> logger)
{
    public async Task<LiveRateResponse> GetRatesAsync(CancellationToken cancellationToken)
    {
        var providerUrl = configuration["LiveRates:ProviderUrl"];
        if (!Uri.TryCreate(providerUrl, UriKind.Absolute, out var providerUri))
        {
            return FailureOrCached("The live-rate provider is not configured.");
        }

        var attempts = Math.Max(1, configuration.GetValue("LiveRates:RetryCount", 3));
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<LiveRateResponse>(
                    providerUri, cancellationToken);

                // An upstream 200 with no data is not a successful rate refresh. Retrying
                // prevents a transient empty query/provider response from replacing good data.
                if (response is { IsSuccess: true, Data.Count: > 0 })
                {
                    var validResponse = response with
                    {
                        LastUpdatedDateTime = response.LastUpdatedDateTime ?? DateTimeOffset.UtcNow,
                        Message = null
                    };
                    cache.Set(validResponse);
                    return validResponse;
                }

                logger.LogWarning(
                    "Live-rate provider returned an unsuccessful or empty response on attempt {Attempt}/{Attempts}.",
                    attempt, attempts);
            }
            catch (Exception exception) when (
                exception is HttpRequestException or System.Text.Json.JsonException ||
                exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception,
                    "Live-rate provider request failed on attempt {Attempt}/{Attempts}.",
                    attempt, attempts);
            }

            if (attempt < attempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }

        return FailureOrCached("Live rates are temporarily unavailable.");
    }

    private LiveRateResponse FailureOrCached(string message)
    {
        var cached = cache.Get();
        if (cached is not null)
        {
            logger.LogWarning("Serving the last known good live rates because the current refresh failed.");
            return cached with { Message = "Showing the last successfully updated rates." };
        }

        // Never claim success when no rates or update time are available.
        return new LiveRateResponse(false, null, [], message);
    }
}
