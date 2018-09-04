using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace MBW.Http.AutoProxy.Cloudflare
{
    internal static class StreamReaderExtensions
    {
        public static IEnumerable<IPNetwork> ReadNetworks(this StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                if (TryParse(line, out IPNetwork network))
                    yield return network;
            }
        }

        private static bool TryParse(string str, out IPNetwork network)
        {
            string[] splits = str.Split('/');
            if (splits.Length != 2)
            {
                network = null;
                return false;
            }

            if (!IPAddress.TryParse(splits[0], out IPAddress ip) || !int.TryParse(splits[1], out int cidr))
            {
                network = null;
                return false;
            }

            network = new IPNetwork(ip, cidr);
            return true;
        }
    }
}