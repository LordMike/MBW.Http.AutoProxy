using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MBW.Http.AutoProxy
{
    public static class AutoProxyExtensions
    {
        public static IAutoProxyConfigurator AddAutoProxyMiddleware(this IServiceCollection services, Action<AutoProxyOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.PostConfigure(configure);

            services.TryAddSingleton<IAutoProxyStore, AutoProxyStore>();

            return new AutoProxyConfigurator(services);
        }

        public static IApplicationBuilder UseAutoProxyMiddleware(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            app.UseMiddleware<AutoProxyMiddleware>();

            return app;
        }
    }
}