using System.Collections.Generic;

namespace MBW.Http.AutoProxy
{
    public class AutoProxyOptions
    {
        public List<string> KnownRanges { get; set; }

        public AutoProxyOptions()
        {
            KnownRanges = new List<string>();
        }
    }
}