using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace MBW.Http.AutoProxy
{
    internal class AutoProxyStore : IAutoProxyStore
    {
        private const string DefaultService = "Default";
        private readonly Dictionary<string, IPNetwork[]> _ranges;
        private List<IPNetwork> _allRanges;

        public event Action OnIpRangesUpdate;

        public AutoProxyStore(IOptionsMonitor<AutoProxyOptions> options, IEnumerable<AutoProxyInitialService> initialServices)
        {
            _ranges = new Dictionary<string, IPNetwork[]>();

            if (options != null)
            {
                if (options.CurrentValue.KnownRanges != null)
                    OnConfigurationChange(options.CurrentValue, null);

                options.OnChange(OnConfigurationChange);
            }

            if (initialServices != null)
            {
                foreach (AutoProxyInitialService initialService in initialServices)
                    initialService.AddRanges(this);
            }
        }

        private void OnConfigurationChange(AutoProxyOptions options, string str)
        {
            IEnumerable<IPNetwork> ranges = options.KnownRanges.Select(s =>
            {
                IPNetworkUtility.TryParse(s, out IPNetwork network);
                return network;
            }).Where(s => s != null);

            ReplaceRanges(DefaultService, ranges);
        }

        public void ReplaceRanges(string service, IEnumerable<IPNetwork> newRanges)
        {
            IPNetwork[] newArray = newRanges.ToArray();

            if (_ranges.TryGetValue(service, out IPNetwork[] oldArray) && oldArray.Length == newArray.Length)
            {
                // Skip if identical
                // https://github.com/aspnet/Configuration/issues/624
                bool skip = true;

                for (int i = 0; i < oldArray.Length; i++)
                {
                    if (IPNetworkUtility.IsEquals(oldArray[i], newArray[i]))
                        continue;

                    skip = false;
                    break;
                }

                if (skip)
                    return;
            }

            _ranges[service] = newArray;

            RebuildInternalStructures();
        }

        private void RebuildInternalStructures()
        {
            // Rebuild all ranges snapshot
            _allRanges = _ranges.Values.SelectMany(s => s).ToList();

            OnIpRangesUpdate?.Invoke();
        }

        public void Clear(bool keepRangesFromOptions = false)
        {
            _ranges.TryGetValue(DefaultService, out IPNetwork[] defaultRanges);
            _ranges.Clear();

            if (keepRangesFromOptions && defaultRanges != null)
                _ranges[DefaultService] = defaultRanges;

            RebuildInternalStructures();
        }

        public IEnumerable<IPNetwork> GetRanges()
        {
            return _allRanges;
        }

        public IEnumerable<KeyValuePair<string, IPNetwork[]>> GetServiceRanges()
        {
            return _ranges.ToList();
        }
    }
}