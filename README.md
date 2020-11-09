## 1. Production
``` csharp
[Fact]
public async Task Production()
{
    var services = new ServiceCollection();
    services.AddAkka(Sys);

    Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

    ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

    ExpectNoMsg();
}
```
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

## 2. UnitTest using MockChildActor
```csharp
[Fact]
public async Task Check_Child_Actor_Recieved_Messages()
{
    var services = new ServiceCollection();
    services.AddSingleton<IActorRef>(sp => TestActor);
    services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();
    services.AddAkka(Sys);

    Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

    ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

    ExpectMsg<string>().Should().Be("Hello, Kid");
    ExpectMsg<string>().Should().Be("Hello, Kid");

    var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
    child1.Path.Name.Should().Be("Child1");

    var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
    child2.Path.Name.Should().Be("Child2");
}
```

