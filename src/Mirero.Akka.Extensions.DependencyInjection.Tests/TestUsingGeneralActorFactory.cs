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
    using Sample;
    using Xunit;

    public class TestUsingGeneralActorFactory : TestKit
    {
        [Fact]
        public async Task Check_Child_Actor_Recieved_Messages()
        {
            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(services =>
                           {
                               services.AddSingleton<IActorRef>(sp => TestActor);
                               services.AddSingleton<IPropsFactory<ChildActor>,
                                   PropsFactory<ChildActor, MockChildActor>>();

                               services.AddAkka(Sys, new[]
                               {
                                   "Sample"
                               });
                           })
                           .Build();

            await host.StartAsync();

            Sys.ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

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
            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(services =>
                           {
                               services.AddAkka(Sys, new[]
                               {
                                   "Sample"
                               });
                           })
                           .Build();

            await host.StartAsync();

            host.Services.GetService<ActorSystem>()
                         .ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

            ExpectNoMsg();

            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");

            await host.StopAsync();
        }
    }
}
