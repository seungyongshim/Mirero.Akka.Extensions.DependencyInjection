namespace Tests
{
    using Akka.Actor;
    using Akka.DI.Core;
    using Akka.TestKit.Xunit2;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using SeungyongShim.Akka.Extensions.DependencyInjection.TestKit;
    using Xunit;

    public class TestUsingGeneralActorFactory : TestKit
    {
        [Fact]
        public void Check_Child_Actor_Recieved_Messages()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ActorSystem>(sp => Sys);
            services.AddSingleton<IActorRef>(sp => TestActor);
            services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();
            services.AddSingleton<IPropsFactory<ParentActor>, PropsFactory<ParentActor>>();
            services.AddTransient<ParentActor>();
            services.AddTransient<MockChildActor>();
            Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().PropsFactory<ParentActor>().Create());

            ExpectMsg<string>().Should().Be("Hello, Kid");
            ExpectMsg<string>().Should().Be("Hello, Kid");
        }

        [Fact]
        public void Production()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ActorSystem>(sp => Sys);
            services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor>>();
            services.AddSingleton<IPropsFactory<ParentActor>, PropsFactory<ParentActor>>();
            services.AddTransient<ParentActor>();
            services.AddTransient<ChildActor>();
            Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().PropsFactory<ParentActor>().Create());

            ExpectNoMsg();
        }

        private class ChildActor : ReceiveActor
        {
            public ChildActor()
            {
                Receive<string>(_ => { });
            }
        }

        private class ParentActor : ReceiveActor
        {
            public ParentActor()
            {
                var childActor1 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create());
                var childActor2 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create());

                childActor1.Tell("Hello, Kid");
                childActor2.Tell("Hello, Kid");
            }
        }
    }
}
