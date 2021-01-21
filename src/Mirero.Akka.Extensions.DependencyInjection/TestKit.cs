using global::Akka.Actor;
using global::Akka.DependencyInjection;
using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

namespace Mirero.Akka.Extensions.DependencyInjection
{
    
    public class PropsFactory<T, R> : IPropsFactory<T> where T : ActorBase where R : ActorBase
    {
        public PropsFactory(ActorSystem actorSystem) => ActorSystem = actorSystem;

        public ActorSystem ActorSystem { get; }

        public Props Create(params object[] args) => ServiceProvider.For(ActorSystem).Props<R>(args);
    }

    public class MockChildActor : ReceiveActor
    {
        public MockChildActor(IActorRef testActor) =>
            ReceiveAny(o => testActor.Forward(o));
    }
}
