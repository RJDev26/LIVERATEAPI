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

## Deploying to IIS

Publish the application rather than copying the project or build directory:

```powershell
dotnet publish .\LiveRateApi\LiveRateApi.csproj -c Release -o C:\Sites\LiveRateApi
```

The published directory includes `web.config`, which registers the ASP.NET Core
Module V2 wildcard handler. Without that handler IIS treats `/api/liverates` as a
physical file path and returns its HTML **404 - File or directory not found** page
before the request reaches ASP.NET Core.

On the server:

1. Install the .NET 8 **Hosting Bundle** (not only the runtime), then restart IIS
   with `iisreset` so `AspNetCoreModuleV2` is loaded.
2. Point the IIS site's physical path at `C:\Sites\LiveRateApi`, the publish output
   containing `LiveRateApi.dll` and `web.config`.
3. If it is deployed below another IIS site, use **Convert to Application** for
   that directory and assign an application pool with **No Managed Code**.
4. Grant the application-pool identity read/execute access to the publish folder.
5. Configure `LiveRates__ProviderUrl` as an IIS environment variable. It must be
   the actual upstream data source and must not point back to this API endpoint.

After publishing, verify locally on the server before testing the public binding:

```powershell
curl.exe -i http://localhost/api/liverates -H "Host: livense.saralaccount.com"
```

An ASP.NET Core JSON response (including a JSON 503 when the provider is down)
confirms that IIS forwarding is working. An IIS-branded HTML 404 means the site
physical path, application conversion, or Hosting Bundle configuration is still
incorrect; that response cannot be fixed inside the endpoint handler because the
request has not reached the application.
