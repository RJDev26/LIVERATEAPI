using LiveRateApi.Models;

namespace LiveRateApi.Services;

public sealed class LiveRateCache
{
    private readonly object _lock = new();
    private LiveRateResponse? _lastGoodResponse;
    private DateTimeOffset _cachedAt;

    internal SemaphoreSlim RefreshLock { get; } = new(1, 1);

    public LiveRateResponse? Get()
    {
        lock (_lock)
        {
            return _lastGoodResponse;
        }
    }

    public LiveRateResponse? GetFresh(TimeSpan lifetime)
    {
        lock (_lock)
        {
            return _lastGoodResponse is not null && DateTimeOffset.UtcNow - _cachedAt < lifetime
                ? _lastGoodResponse
                : null;
        }
    }

    public void Set(LiveRateResponse response)
    {
        if (!response.IsSuccess || response.Data.Count == 0)
        {
            throw new ArgumentException("Only successful, non-empty responses may be cached.", nameof(response));
        }

        lock (_lock)
        {
            _lastGoodResponse = response;
            _cachedAt = DateTimeOffset.UtcNow;
        }
    }
}
