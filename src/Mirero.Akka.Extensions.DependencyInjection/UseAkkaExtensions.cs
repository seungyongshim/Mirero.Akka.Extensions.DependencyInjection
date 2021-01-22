namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Akka.Actor;
    using Akka.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

    
    public static class UseAkkaExtensions
    {
        public static IHostBuilder UseAkka(this IHostBuilder host,
                                           Action<ActorSystem> startAction = null,
                                           IEnumerable<string> autoExcludeAssemblies = null)
        {
            return host.ConfigureServices((context, services) =>
            {
                services.AddAkka(startAction, autoExcludeAssemblies);
            });
        }
        internal static IServiceCollection AddAkka(this IServiceCollection services,
                                                 Action<ActorSystem> startAction = null,
                                                 IEnumerable<string> autoExcludeAssemblies = null)
        {
            services.AddSingleton<AkkaHostedServiceStart>(sp => x => startAction?.Invoke(x));
            services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
            services.AddHostedService<AkkaHostedService>();

            return services;
        }
    }
}
