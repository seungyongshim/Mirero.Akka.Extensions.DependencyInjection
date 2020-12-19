namespace Tests
{
    using System.Threading.Tasks;
    using Akka.Actor;
    using Akka.DI.Core;
    using Akka.TestKit.Xunit2;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mirero.Akka.Extensions.DependencyInjection;
    using Mirero.Akka.Extensions.DependencyInjection.Abstractions;
    using Xunit;

    public class ParentActor : ReceiveActor
    {
        public ParentActor()
        {
            var childActor1 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child1");
            var childActor2 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child2");

            Receive<string>(msg =>
            {
                childActor1.Tell($"{msg}, Kid");
                childActor2.Tell($"{msg}, Kid");
            });
        }
    }
    public class ChildActor : ReceiveActor
    {
        public ChildActor() => ReceiveAny(_ => { });
    }

    public class TestUsingGeneralActorFactory : TestKit
    {
        [Fact]
        public async Task Check_Child_Actor_Recieved_Messages()
        {
            // Arrange
            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(services =>
                           {
                               services.AddSingleton<IActorRef>(sp => TestActor);
                               services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();

                               services.AddAkka(Sys);
                           })
                           .Build();

            await host.StartAsync();

            IActorRef parentActor = Sys.ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

            // Act
            parentActor.Tell("Hello");

            // Assert
            ExpectMsg<string>().Should().Be("Hello, Kid");
            ExpectMsg<string>().Should().Be("Hello, Kid");

            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");

            await host.StopAsync();
        }

        [Fact]
        public async Task Production()
        {
            // Arrange
            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(services =>
                           {
                               services.AddAkka(Sys, sys =>
                               {
                                   sys.ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");
                               });
                           })
                           .Build();

            await host.StartAsync();

            // Act
            ActorSelection("/user/Parent").Tell("Hello");

            // Assert
            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");

            await host.StopAsync();
        }
    }
}
