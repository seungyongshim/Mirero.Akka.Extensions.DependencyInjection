![CI](https://github.com/seungyongshim/Mirero.Akka.Extensions.DependencyInjection/workflows/CI/badge.svg)

```csharp
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
```

## 1. Production
``` csharp
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
    ExpectNoMsg();

    await host.StopAsync();
}
```

## 2. UnitTest using MockChildActor
```csharp
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
                       services.AddSingleton(sp => sp.GetService<TestKit>().Sys);
                       services.AddSingleton(sp => sp.GetService<TestKit>().TestActor);
                       services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();
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
    testKit.ExpectMsg<string>().Should().Be("Hello, Kid");
    testKit.ExpectMsg<string>().Should().Be("Hello, Kid");

    var child1 = await testKit.Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
    child1.Path.Name.Should().Be("Child1");

    var child2 = await testKit.Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
    child2.Path.Name.Should().Be("Child2");

    await host.StopAsync();
}
```

