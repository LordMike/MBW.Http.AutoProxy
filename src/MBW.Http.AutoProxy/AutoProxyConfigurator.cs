using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace MBW.Http.AutoProxy
{
    internal class AutoProxyConfigurator : IAutoProxyConfigurator
    {
        private readonly IServiceCollection _services;

        public AutoProxyConfigurator(IServiceCollection services)
        {
            _services = services;
        }

        public IAutoProxyConfigurator AddInitialRanges(string service, IEnumerable<IPNetwork> range)
        {
            _services.AddSingleton(new AutoProxyInitialService(service, range.ToList()));
            return this;
        }

        public IAutoProxyConfigurator AddServices(Action<IServiceCollection> services)
        {
            services(_services);
            return this;
        }
    }
}