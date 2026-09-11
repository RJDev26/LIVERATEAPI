# Live Rate API

This API prevents a transient empty provider response from being reported as a
successful live-rate response.

Set `LiveRates__ProviderUrl` to the upstream endpoint, then run the application:

```sh
dotnet run --project LiveRateApi
```

`GET /api/liverates` retries unsuccessful or empty provider results. A successful,
non-empty result becomes the in-memory last-known-good value. If a later refresh
fails, that value is returned instead. Before the first successful refresh, an
empty result is returned with HTTP 503 and `IsSuccess: false`; the API never emits
`IsSuccess: true` together with an empty `Data` array.
