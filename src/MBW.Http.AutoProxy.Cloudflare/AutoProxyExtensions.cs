using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace MBW.Http.AutoProxy.Cloudflare
{
    public static class AutoProxyExtensions
    {
        public static IAutoProxyConfigurator AddCloudflare(this IAutoProxyConfigurator configurator)
        {
            Assembly assembly = typeof(AutoProxyExtensions).Assembly;
            using (Stream fs = assembly.GetManifestResourceStream(assembly.GetName().Name + ".Resources.ips.txt"))
            using (StreamReader sr = new StreamReader(fs))
                configurator.AddInitialRanges(Constants.ServiceName, sr.ReadNetworks());

            return configurator;
        }

        public static IAutoProxyConfigurator AddCloudflareUpdater(this IAutoProxyConfigurator configurator, Action<CloudflareUpdaterOptions> configureOptions = null)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.AddServices(services =>
            {
                if (configureOptions != null)
                    services.PostConfigure(configureOptions);

                services.AddSingleton<IHostedService>(provider => new CloudflareIpUpdater(
                    provider.GetRequiredService<ILogger<CloudflareIpUpdater>>(),
                    provider.GetRequiredService<IAutoProxyStore>(),
                    provider.GetRequiredService<IOptionsMonitor<CloudflareUpdaterOptions>>(),
                    provider.GetRequiredService<IHttpClientFactory>().CreateClient(typeof(CloudflareIpUpdater).FullName)));
                services.AddHttpClient(typeof(CloudflareIpUpdater).FullName, (provider, client) =>
                    {
                        client.BaseAddress = new Uri("https://www.cloudflare.com/");
                    })
                    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    }));
            });

            return configurator;
        }
    }
}