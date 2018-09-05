using System;
using System.Collections.Generic;
using System.Linq;
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
        public const string HeaderXForwardedFor = "X-Forwarded-For";
        public const string HeaderXForwardedProto = "X-Forwarded-Proto";

        private readonly ILogger<AutoProxyMiddleware> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RequestDelegate _next;
        private readonly IAutoProxyStore _autoProxyStore;
        private ForwardedHeadersMiddleware _forwardedHeadersMiddleware;

        public AutoProxyMiddleware(ILoggerFactory loggerFactory, RequestDelegate next, IAutoProxyStore autoProxyStore)
        {
            _logger = loggerFactory.CreateLogger<AutoProxyMiddleware>();
            _loggerFactory = loggerFactory;
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _autoProxyStore = autoProxyStore;

            autoProxyStore.OnIpRangesUpdate += CloudflareIpsOnOnIpRangesUpdate;

            ReplaceKnownProxies(autoProxyStore.GetRanges());
        }

        private void CloudflareIpsOnOnIpRangesUpdate()
        {
            ReplaceKnownProxies(_autoProxyStore.GetRanges());
        }

        public void ReplaceKnownProxies(IEnumerable<IPNetwork> ranges)
        {
            ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedForHeaderName = HeaderXForwardedFor,
                ForwardedProtoHeaderName = HeaderXForwardedProto,
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();

            foreach (IPNetwork network in ranges)
                forwardedHeadersOptions.KnownNetworks.Add(network);

            _logger.LogInformation("Changing auto proxy networks to {Networks}", forwardedHeadersOptions.KnownNetworks.Select(s => $"{s.Prefix}/{s.PrefixLength}"));

            _forwardedHeadersMiddleware = new ForwardedHeadersMiddleware(_next, _loggerFactory, Options.Create(forwardedHeadersOptions));
        }

        public Task Invoke(HttpContext context)
        {
            return _forwardedHeadersMiddleware.Invoke(context);
        }
    }
}