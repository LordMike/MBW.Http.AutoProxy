using System.Collections.Generic;

namespace MBW.Http.AutoProxy
{
    public class AutoProxyOptions
    {
        public List<string> KnownRanges { get; set; }

        /// <summary>
        /// In Dual-stack environments, the IPv4 addresses of clients are nested in IPv6 addresses, and the KnownNetworks/KnownProxies must also be in IPv6.
        /// This options silently converts IPv4 to IPv6 and supports both kinds in one go.
        /// Default: true
        /// </summary>
        public bool AutoConvertIPv4ToIPv6 { get; set; }

        /// <summary>
        /// Uses the X-Forwarded-Host header (request hostname)
        /// Default: false
        /// </summary>
        public bool UseForwardedHost { get; set; }

        /// <summary>
        /// Uses the X-Forwarded-For header (client ip)
        /// Default: true
        /// </summary>
        public bool UseForwardedFor { get; set; }

        /// <summary>
        /// Uses the X-Forwarded-Proto header (request scheme - "http" / "https")
        /// Default: true
        /// </summary>
        public bool UseForwardedProto { get; set; }

        public AutoProxyOptions()
        {
            KnownRanges = new List<string>();
            AutoConvertIPv4ToIPv6 = true;
            UseForwardedFor = true;
            UseForwardedProto = true;
        }
    }
}