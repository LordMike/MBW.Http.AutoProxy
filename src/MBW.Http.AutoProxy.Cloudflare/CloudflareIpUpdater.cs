using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MBW.Http.AutoProxy.Cloudflare
{
    internal class CloudflareIpUpdater : IHostedService
    {
        private readonly ILogger<CloudflareIpUpdater> _logger;
        private readonly IAutoProxyStore _proxyStore;
        private readonly IOptionsMonitor<CloudflareUpdaterOptions> _options;
        private readonly HttpClient _httpClient;

        // Provided by David Fowler: https://gist.github.com/davidfowl/a7dd5064d9dcf35b6eae1a7953d615e3

        private Task _executingTask;
        private CancellationTokenSource _cts;

        internal CloudflareIpUpdater(ILogger<CloudflareIpUpdater> logger, IAutoProxyStore proxyStore, IOptionsMonitor<CloudflareUpdaterOptions> options, HttpClient httpClient)
        {
            _logger = logger;
            _proxyStore = proxyStore;
            _options = options;
            _httpClient = httpClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Store the task we're executing
            _executingTask = ExecuteAsync(_cts.Token)
                .ContinueWith(task => _logger.LogError(task.Exception, "An error occurred in a background task"), TaskContinuationOptions.OnlyOnFaulted);

            // If the task is completed then return it, otherwise it's running
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
                return;

            // Signal cancellation to the executing method
            _cts.Cancel();

            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken)).ConfigureAwait(false);

            // Throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_options.CurrentValue.Interval, cancellationToken);

                if (!_options.CurrentValue.Enabled)
                    continue;

                _logger.LogDebug("Beginning Cloudflare IP Retrieval");

                try
                {
                    Task<HttpResponseMessage> respV4Task = _httpClient.GetAsync("ips-v4", cancellationToken);
                    Task<HttpResponseMessage> respV6Task = _httpClient.GetAsync("ips-v6", cancellationToken);

                    await Task.WhenAll(respV4Task, respV6Task);

                    HttpResponseMessage respV4 = respV4Task.Result;
                    HttpResponseMessage respV6 = respV6Task.Result;

                    if (respV4.IsSuccessStatusCode && respV6.IsSuccessStatusCode)
                    {
                        List<IPNetwork> subnets = new List<IPNetwork>();

                        using (StreamReader sr = new StreamReader(await respV4.Content.ReadAsStreamAsync()))
                            subnets.AddRange(sr.ReadNetworks());

                        using (StreamReader sr = new StreamReader(await respV6.Content.ReadAsStreamAsync()))
                            subnets.AddRange(sr.ReadNetworks());

                        // Success
                        _proxyStore.ReplaceRanges(Constants.ServiceName, subnets);

                        _logger.LogInformation("Successfully retrieved {RangeCount} new Cloudflare IP Ranges", subnets.Count);
                        _logger.LogDebug("New Cloudflare Ranges: {Ranges}", subnets.Select(s => $"{s.Prefix}/{s.PrefixLength}"));
                    }
                    else
                    {
                        _logger.LogWarning("Unable to retrieve Cloudflare IPs. V4: {ResultV4}, V6: {ResultV6}", respV4, respV6);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error updating Cloudflare IPs");
                }

                _logger.LogDebug("Completed Cloudflare IP Retrieval");
            }
        }
    }
}