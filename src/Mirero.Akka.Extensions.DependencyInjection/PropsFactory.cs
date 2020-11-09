namespace Microsoft.Extensions.DependencyInjection
{
    using Akka.Actor;
    using Akka.DI.Core;

    public class PropsFactory<T> : IPropsFactory<T> where T : ActorBase
    {
        public PropsFactory(ActorSystem actorSystem) => ActorSystem = actorSystem;

        public ActorSystem ActorSystem { get; }

        public Props Create() => ActorSystem.DI().Props<T>();
    }
}
