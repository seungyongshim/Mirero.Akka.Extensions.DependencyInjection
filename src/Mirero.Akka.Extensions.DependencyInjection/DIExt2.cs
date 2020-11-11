namespace Mirero.Akka.Extensions.DependencyInjection
{
    using System;
    using global::Akka.Actor;
    using Microsoft.Extensions.DependencyInjection;
    using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

    public class DIExt2 : IExtension
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public void Initialize(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

        public IPropsFactory<T> PropsFactory<T>() where T : ActorBase =>
            ServiceProvider.GetRequiredService<IPropsFactory<T>>();
    }
}
