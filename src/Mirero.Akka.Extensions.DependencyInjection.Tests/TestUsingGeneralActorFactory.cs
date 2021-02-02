using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akka.DependencyInjection;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mirero.Akka.Extensions.DependencyInjection;
using Mirero.Akka.Extensions.DependencyInjection.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Akka.Actor.Setup;
using System;
using System.Linq;

namespace Tests
{
    public interface ILogic { }

    public class Logic : ILogic { }

    public class ParentActor : ReceiveActor
    {
        public ParentActor()
        {
            var childActor1 = Context.ActorOf(Context.PropsFactory<ChildActor>().Create(Self), "Child1");
            var childActor2 = Context.ActorOf(Context.PropsFactory<ChildActor>().Create(Self), "Child2");

            Receive<string>(msg =>
            {
                childActor1.Tell($"{msg}, Kid");
                childActor2.Tell($"{msg}, Kid");
            });
        }
    }
    public class ChildActor : ReceiveActor
    {
        public ChildActor(IServiceProvider sp, ILogic logic, IActorRef actorRef) => ReceiveAny(_ => { });
    }

    public class TestUsingGeneralActorFactory 
    {
        [Fact]
        public async Task Check_Child_Actor_Recieved_Messages()
        {
            // Arrange
            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(services =>
                           {
                               services.AddSingleton(sp =>
                                   new TestKit(BootstrapSetup.Create()
                                                             .And(ServiceProviderSetup.Create(sp))));
                               services.AddTransient<ILogic, Logic>();
                           })
                           .UseAkka(sys =>
                           {
                               sys.ActorOf(sys.PropsFactory<ParentActor>().Create(), "Parent");
                           }, new[]
                           {
                               typeof(ChildActor),
                           })
                           .Build();

            await host.StartAsync();

            var testKit = host.Services.GetService<TestKit>();

            // Act
            testKit.ActorSelection("/user/Parent").Tell("Hello");

            // Assert
            testKit.ExpectMsg<string>().Should().Be("Hello, Kid");
            testKit.ExpectMsg<string>().Should().Be("Hello, Kid");

            var child1 = await testKit.Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await testKit.Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
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
                               services.AddSingleton(sp =>
                                   new TestKit(BootstrapSetup.Create()
                                                             .And(ServiceProviderSetup.Create(sp))));
                               services.AddSingleton(sp => sp.GetService<TestKit>().Sys);

                               services.AddTransient<ILogic, Logic>();
                               
                           })
                           .UseAkka(sys =>
                           {
                               sys.ActorOf(sys.PropsFactory<ParentActor>().Create(), "Parent");
                           })
                           .Build();

            await host.StartAsync();
            var testKit = host.Services.GetService<TestKit>();

            // Act
            testKit.ActorSelection("/user/Parent").Tell("Hello");

            // Assert
            var child1 = await testKit.Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await testKit.Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");

            await host.StopAsync();
        }
    }
}
