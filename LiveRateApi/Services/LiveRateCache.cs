using LiveRateApi.Models;

namespace LiveRateApi.Services;

public sealed class LiveRateCache
{
    private readonly object _lock = new();
    private LiveRateResponse? _lastGoodResponse;

    public LiveRateResponse? Get()
    {
        lock (_lock)
        {
            return _lastGoodResponse;
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
        }
    }
}
