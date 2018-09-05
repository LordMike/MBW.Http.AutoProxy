using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.HttpOverrides;

namespace MBW.Http.AutoProxy
{
    public interface IAutoProxyStore
    {
        event Action OnIpRangesUpdate;
        void ReplaceRanges(string service, IEnumerable<IPNetwork> newRanges);
        void Clear(bool keepRangesFromOptions = false);
        IEnumerable<IPNetwork> GetRanges();
        IEnumerable<KeyValuePair<string, IPNetwork[]>> GetServiceRanges();
    }
}