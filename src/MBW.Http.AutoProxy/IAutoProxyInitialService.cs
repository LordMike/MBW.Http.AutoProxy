using System.Collections.Generic;
using Microsoft.AspNetCore.HttpOverrides;

namespace MBW.Http.AutoProxy
{
    public class AutoProxyInitialService
    {
        private readonly string _service;
        private readonly ICollection<IPNetwork> _ranges;

        public AutoProxyInitialService(string service, ICollection<IPNetwork> ranges)
        {
            _service = service;
            _ranges = ranges;
        }

        public void AddRanges(AutoProxyStore store)
        {
            store.ReplaceRanges(_service, _ranges);
        }
    }
}