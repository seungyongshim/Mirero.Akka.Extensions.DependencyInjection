using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.TestKit.Xunit2;
using Microsoft.Extensions.Hosting;
using Mirero.Akka.Extensions.DependencyInjection;
using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public delegate IActorRef GetTestActor();

    public static class UseAkkaTestKitExtensions
    {
        public static IHostBuilder UseAkkaXUnit2(this IHostBuilder host) =>
            host.UseAkkaXUnit2(Enumerable.Empty<Type>());

        public static IHostBuilder UseAkkaXUnit2(this IHostBuilder host,
                                           IEnumerable<Type> mockActors = null)
        {
            return host.ConfigureServices((context, services) =>
            {
                services.AddAkkaTestKit(mockActors ?? Enumerable.Empty<Type>());
            });
        }

        private static IServiceCollection AddAkkaTestKit(this IServiceCollection services, IEnumerable<Type> mockActors)
        {
            services.AddSingleton(sp => new TestKit(sp.GetService<ActorSystemSetup>()));
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
