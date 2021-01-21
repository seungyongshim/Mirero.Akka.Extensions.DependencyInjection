using Akka.DependencyInjection;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Mirero.Akka.Extensions.DependencyInjection.Abstractions;


namespace Mirero.Akka.Extensions.DependencyInjection
{
    public static class ServiceProviderExtension
    {
        public static IPropsFactory<T> PropsFactory<T>(this ActorSystem actorSystem) where T : ActorBase
        {
            return ServiceProvider.For(actorSystem).Provider.GetRequiredService<IPropsFactory<T>>();
        }

        public static IPropsFactory<T> PropsFactory<T>(this IUntypedActorContext context) where T : ActorBase
        {
            return ServiceProvider.For(context.System).Provider.GetRequiredService<IPropsFactory<T>>();
        }
    }
}
