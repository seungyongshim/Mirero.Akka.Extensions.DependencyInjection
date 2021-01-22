using System;
using System.Linq;
using global::Akka.Actor;
using global::Akka.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
        public MockChildActor(IServiceProvider sp) : this(sp, default) { }
        public MockChildActor(IServiceProvider sp, object o1) : this(sp, o1, default) { }
        public MockChildActor(IServiceProvider sp, object o1, object o2) : this(sp, o1, o2, default) { }
        public MockChildActor(IServiceProvider sp, object o1, object o2, object o3)
        {
            var testActor = sp.GetService<IActorRef>();
            ReceiveAny(o => testActor.Forward(o));
        }
    }
}
