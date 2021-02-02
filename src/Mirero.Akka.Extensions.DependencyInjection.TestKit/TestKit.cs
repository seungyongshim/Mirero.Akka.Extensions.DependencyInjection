using System;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

namespace Mirero.Akka.Extensions.DependencyInjection
{
    
    internal class PropsFakeFactory<T> : IPropsFactory<T> where T : ActorBase
    {
        public PropsFakeFactory(ActorSystem actorSystem) => ActorSystem = actorSystem;

        public ActorSystem ActorSystem { get; }

        public Props Create(params object[] args) => ServiceProvider.For(ActorSystem).Props<FakeActor>(args);
    }

    internal class FakeActor : ReceiveActor
    {
        public FakeActor(IServiceProvider sp) : this(sp, default) { }
        public FakeActor(IServiceProvider sp, object o1) : this(sp, o1, default) { }
        public FakeActor(IServiceProvider sp, object o1, object o2) : this(sp, o1, o2, default) { }
        public FakeActor(IServiceProvider sp, object o1, object o2, object o3)
        {
            var testActor = sp.GetService<GetTestActor>()();
            ReceiveAny(testActor.Forward);
        }
    }
}
