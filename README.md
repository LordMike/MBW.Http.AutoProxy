# MBW.Http.AutoProxy [![Generic Build](https://github.com/LordMike/MBW.Http.AutoProxy/actions/workflows/dotnet.yml/badge.svg)](https://github.com/LordMike/MBW.Http.AutoProxy/actions/workflows/dotnet.yml) [![Nuget](https://img.shields.io/nuget/v/MBW.Http.AutoProxy)](https://www.nuget.org/packages/MBW.Http.AutoProxy)

A utility to ease configuration of proxy middlewares in ASP.Net, to read in `X-Forward-For` and `X-Forwarded-Proto` from one or more trusted proxies.

### Features

* One-method add of proxies, with no additional configuration
* Possible to add in own sources for trusted proxies
* Known proxy lists, f.ex. Cloudflare
* Ability to auto-update proxy lists, should they change

### Packages

| Package | Nuget |
| ------------- |:-------------:|
| MBW.Http.AutoProxy | [![NuGet](https://img.shields.io/nuget/v/MBW.Http.AutoProxy.svg)](https://www.nuget.org/packages/MBW.Http.AutoProxy) |
| MBW.Http.AutoProxy.Cloudflare | [![NuGet](https://img.shields.io/nuget/v/MBW.Http.AutoProxy.Cloudflare.svg)](https://www.nuget.org/packages/MBW.Http.AutoProxy.Cloudflare) |

### Usage

In your `ConfigureService`, use the `AddAutoProxyMiddleware()` helper, and then add in any sources you'd like to use, f.ex. `Cloudflare`. 

```csharp
    services.AddAutoProxyMiddleware()
```

In your app pipeline, in the `Configure` method, add the middleware as soon as possible:

```csharp
    app.UseAutoProxyMiddleware();
```

#### Add Cloudflare

To add in the initial configuration (a hardcoded set of IP's that will update with the nuget packages), use `AddCloudflare()`. To get the auto-updater that will query the Cloudflare ip lists once every while, use `AddCloudflareUpdater()`.

```csharp
    services.AddAutoProxyMiddleware()
        .AddCloudflare()
        .AddCloudflareUpdater();
```

#### Extend with extra sources

Take a look at the `AutoProxyExtensions` class in the Cloudflare package, it shows how to call into the `IAutoProxyConfigurator` service and add information. In short, you need to call `IAutoProxyConfigurator.AddInitialRanges()` to add an initial, hard-coded, configuration, and then `IAutoProxyStore.ReplaceRanges()` when you have an update.
