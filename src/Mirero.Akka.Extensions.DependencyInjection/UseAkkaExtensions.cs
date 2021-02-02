using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Mirero.Akka.Extensions.DependencyInjection.XUnit2")]

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Akka.Actor;
    using Akka.Actor.Setup;
    using Akka.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mirero.Akka.Extensions.DependencyInjection.Abstractions;


    public static class UseAkkaExtensions
    {
        public static IHostBuilder UseAkka(this IHostBuilder host,
                                           string actorSystemName,
                                           Action<ActorSystem> startAction = null)
        {
            return host.ConfigureServices((context, services) =>
            {
                services.AddAkka(actorSystemName, startAction);
            });
        }
        internal static IServiceCollection AddAkka(this IServiceCollection services,
                                                   string actorSystemName,
                                                   Action<ActorSystem> startAction = null)
        {
            services.AddSingleton<AkkaHostedServiceStart>(sp => x => startAction?.Invoke(x));
            services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
            services.AddHostedService<AkkaHostedService>();
            services.AddSingleton(sp => ActorSystem.Create(actorSystemName, sp.GetService<ActorSystemSetup>()));


            return services;
        }
    }
}
