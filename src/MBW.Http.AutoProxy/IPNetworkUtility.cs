using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace MBW.Http.AutoProxy
{
    public static class IPNetworkUtility
    {
        public static bool TryParse(string str, out IPNetwork network)
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

        public static bool IsEquals(IPNetwork a, IPNetwork b)
        {
            return Equals(a.Prefix, b.Prefix) && a.PrefixLength == b.PrefixLength;
        }
    }
}