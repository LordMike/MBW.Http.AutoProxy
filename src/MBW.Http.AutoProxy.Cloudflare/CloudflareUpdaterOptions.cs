using System;

namespace MBW.Http.AutoProxy.Cloudflare
{
    public class CloudflareUpdaterOptions
    {
        public bool Enabled { get; set; }

        public TimeSpan Interval { get; set; }

        public CloudflareUpdaterOptions()
        {
            Interval = TimeSpan.FromDays(2);
            Enabled = true;
        }
    }
}