namespace Akka.DI.Core
{
    using Akka.Actor;
    using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

    public class PropsFactory<T> : IPropsFactory<T> where T : ActorBase
    {
        public PropsFactory(ActorSystem actorSystem) => ActorSystem = actorSystem;

        public ActorSystem ActorSystem { get; }

        public Props Create() => ActorSystem.DI().Props<T>();
    }
}
