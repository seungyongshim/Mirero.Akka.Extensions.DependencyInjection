namespace SeungyongShim.Akka.Extensions.DependencyInjection.TestKit
{
    using global::Akka.Actor;
    using global::Akka.DI.Core;
    using Microsoft.Extensions.DependencyInjection;

    public class PropsFactory<T, R> : IPropsFactory<T> where T : ActorBase where R : ActorBase
    {
        public PropsFactory(ActorSystem actorSystem) => ActorSystem = actorSystem;

        public ActorSystem ActorSystem { get; }

        public Props Create() => ActorSystem.DI().Props<R>();
    }
    public class MockChildActor : ReceiveActor
    {
        public MockChildActor(IActorRef testActor) =>
            ReceiveAny(o => testActor.Forward(o));
    }
}
