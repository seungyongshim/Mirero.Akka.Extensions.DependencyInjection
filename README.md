![CI](https://github.com/seungyongshim/Mirero.Akka.Extensions.DependencyInjection/workflows/CI/badge.svg)

```csharp
class ParentActor : ReceiveActor
{
    public ParentActor()
    {
        var childActor1 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child1");
        var childActor2 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child2");
        
        childActor1.Tell("Hello, Kid");
        childActor2.Tell("Hello, Kid");
    }
}
```

## 1. Production
``` csharp
[Fact]
public async Task Production()
{
    // arrange
    var host = Host.CreateDefaultBuilder()
                   .ConfigureServices(services =>
                   {
                       services.AddAkka(Sys, sys =>
                       {
                           sys.ActorOf(sys.DI().PropsFactory<ParentActor>().Create(), "Parent");
                       });
                   })
                   .Build();

    await host.StartAsync();
    
    // assert
    ExpectNoMsg();

    await host.StopAsync();
}
```

## 2. UnitTest using MockChildActor
```csharp
[Fact]
public async Task Check_Child_Actor_Recieved_Messages()
{
    var host = Host.CreateDefaultBuilder()
                   .ConfigureServices(services =>
                   {
                       services.AddSingleton<IActorRef>(sp => TestActor);
                       services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();

                       services.AddAkka(Sys, sys =>
                       {
                           sys.ActorOf(sys.DI().PropsFactory<ParentActor>().Create(), "Parent");
                       });
                   })
                   .Build();

    await host.StartAsync();

    ExpectMsg<string>().Should().Be("Hello, Kid");
    ExpectMsg<string>().Should().Be("Hello, Kid");

    var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
    child1.Path.Name.Should().Be("Child1");

    var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
    child2.Path.Name.Should().Be("Child2");

    await host.StopAsync();
}
```

