using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace MBW.Http.AutoProxy
{
    public interface IAutoProxyConfigurator
    {
        IAutoProxyConfigurator AddInitialRanges(string service, IEnumerable<IPNetwork> range);
        IAutoProxyConfigurator AddServices(Action<IServiceCollection> services);
    }
}