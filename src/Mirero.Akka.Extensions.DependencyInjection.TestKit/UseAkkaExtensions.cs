using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Microsoft.Extensions.Hosting;
using Mirero.Akka.Extensions.DependencyInjection;
using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public delegate IActorRef GetTestActor();

    public static class UseAkkaTestKitExtensions
    {
        public static IHostBuilder UseAkka(this IHostBuilder host,
                                           Action<ActorSystem> startAction,
                                           bool useTestKit)
        {
            return useTestKit ? host.UseAkka(startAction, Enumerable.Empty<Type>())
                              : host.UseAkka(startAction);
        }
        public static IHostBuilder UseAkka(this IHostBuilder host,
                                           Action<ActorSystem> startAction = null,
                                           IEnumerable<Type> mockActors = null)
        {
            return host.ConfigureServices((context, services) =>
            {
                services.AddAkka(startAction);
                services.UseAkkaTestKit(mockActors ?? Enumerable.Empty<Type>());
            });
        }

        private static IServiceCollection UseAkkaTestKit(this IServiceCollection services, IEnumerable<Type> mockActors)
        {
            services.AddSingleton<GetTestActor>(sp => () => sp.GetService<TestKit>().TestActor);
            services.AddSingleton(sp => sp.GetService<TestKit>().Sys);

            foreach (var item in mockActors)
            {
                services.AddSingleton(typeof(IPropsFactory<>).MakeGenericType(item),
                                      typeof(PropsFakeFactory<>).MakeGenericType(item));
            }

            return services;
        }
    }
}
