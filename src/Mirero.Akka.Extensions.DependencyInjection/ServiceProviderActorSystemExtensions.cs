namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Akka.Actor;
    using Akka.DI.Extensions.DependencyInjection;
    using SeungyongShim.Akka.Extensions.DependencyInjection;

    public static class ServiceProviderActorSystemExtensions
    {
        public static ActorSystem UseDependencyInjectionServiceProvider(this ActorSystem system, IServiceProvider serviceProvider)
        {
            system.RegisterExtension(DIExtension2.DIExtensionProvider);
            DIExtension2.DIExtensionProvider.Get(system).Initialize(serviceProvider);

            var _ = new ServiceProviderDependencyResolver(serviceProvider, system);
            return system;
        }
    }
}




