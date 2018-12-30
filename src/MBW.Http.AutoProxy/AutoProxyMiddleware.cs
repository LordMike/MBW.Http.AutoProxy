using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MBW.Http.AutoProxy
{
    internal class AutoProxyMiddleware
    {
        public static readonly string HeaderXForwardedFor = ForwardedHeadersDefaults.XForwardedForHeaderName;
        public static readonly string HeaderXForwardedProto = ForwardedHeadersDefaults.XForwardedProtoHeaderName;
        public static readonly string HeaderXForwardedHost = ForwardedHeadersDefaults.XForwardedHostHeaderName;

        private readonly ILogger<AutoProxyMiddleware> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptionsMonitor<AutoProxyOptions> _options;
        private readonly RequestDelegate _next;
        private readonly IAutoProxyStore _autoProxyStore;
        private ForwardedHeadersMiddleware _forwardedHeadersMiddleware;

        public AutoProxyMiddleware(ILoggerFactory loggerFactory, IOptionsMonitor<AutoProxyOptions> options, RequestDelegate next, IAutoProxyStore autoProxyStore)
        {
            _logger = loggerFactory.CreateLogger<AutoProxyMiddleware>();
            _loggerFactory = loggerFactory;
            _options = options;
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _autoProxyStore = autoProxyStore;

            // Register event handlers
            _options.OnChange(_ => OnIpRangesUpdate());
            autoProxyStore.OnIpRangesUpdate += OnIpRangesUpdate;

            ReplaceKnownProxies(autoProxyStore.GetRanges());
        }

        private void OnIpRangesUpdate()
        {
            ReplaceKnownProxies(_autoProxyStore.GetRanges());
        }

        public void ReplaceKnownProxies(IEnumerable<IPNetwork> ranges)
        {
            AutoProxyOptions options = _options.CurrentValue;

            ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedForHeaderName = HeaderXForwardedFor,
                ForwardedProtoHeaderName = HeaderXForwardedProto,
                ForwardedHostHeaderName = HeaderXForwardedHost,
                ForwardedHeaders = ForwardedHeaders.None
            };

            if (options.UseForwardedFor)
                forwardedHeadersOptions.ForwardedHeaders |= ForwardedHeaders.XForwardedFor;

            if (options.UseForwardedHost)
                forwardedHeadersOptions.ForwardedHeaders |= ForwardedHeaders.XForwardedHost;

            if (options.UseForwardedProto)
                forwardedHeadersOptions.ForwardedHeaders |= ForwardedHeaders.XForwardedProto;

            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();

            foreach (IPNetwork network in ranges)
            {
                forwardedHeadersOptions.KnownNetworks.Add(network);

                // Convert IPv4 addresses to IPv6 if needed
                if (!options.AutoConvertIPv4ToIPv6 || network.Prefix.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                IPNetwork newNetwork = new IPNetwork(network.Prefix.MapToIPv6(), network.PrefixLength + 96);
                forwardedHeadersOptions.KnownNetworks.Add(newNetwork);
            }

            _logger.LogInformation("Changing auto proxy networks to {Networks}", forwardedHeadersOptions.KnownNetworks.Select(s => $"{s.Prefix}/{s.PrefixLength}"));

            _forwardedHeadersMiddleware = new ForwardedHeadersMiddleware(_next, _loggerFactory, Options.Create(forwardedHeadersOptions));
        }

        public Task Invoke(HttpContext context)
        {
            return _forwardedHeadersMiddleware.Invoke(context);
        }
    }
}