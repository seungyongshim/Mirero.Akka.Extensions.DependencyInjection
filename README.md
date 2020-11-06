## 1. 참조
- https://livebook.manning.com/book/dependency-injection-principles-practices-patterns/chapter-15/
- https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/june/essential-net-dependency-injection-with-net-core
- https://sachabarbs.wordpress.com/2018/05/22/akka-net-di-testing-considerations/
- https://github.com/seungyongshim/POC-Akka-Mock-Child-UnitTest

## 2. 작성 방법 (ChildActor 상속 받은 MockChildActor를 사용)
- [-단점: ChildActor를 상속 받았기 때문에 ChildActor의 생성자가 단위 테스트에서 실행된다.-]
```csharp
[Fact]
public void Has_Not_Child_Factory()
{
    var services = new ServiceCollection();
    // MockChildActor에서 사용하는 TestActor 의존성을 선언한다.
    services.AddSingleton<IActorRef>(sp => TestActor);
    services.AddTransient<ChildActor, MockChildActor>();
    Sys.UseServiceProvider(services.BuildServiceProvider());

    ActorOf<ParentActor>();

    ExpectMsg<string>().Should().Be("Hello, Kid");
}
```
```csharp
class ParentActor : ReceiveActor
{
    public ParentActor()
    {
        var childActor = Context.ActorOf(Context.DI().Props<ChildActor>());
        childActor.Tell("Hello, Kid");
    }
}
```
```csharp
class ChildActor : ReceiveActor
{
    public ChildActor()
    {
        // MockChildActor 생성자에서 호출 된다.
        // 여기 작성된 내용은 MockChildActor에도 반영된다.
        Receive<string>(_ => { });
    }
}
```
```csharp
class MockChildActor : ChildActor
{
    public MockChildActor(IActorRef testActor)
    {
        // ChildActor 생성자에서 선언한 Receive를 오버라이딩 한다
        Become(() =>
        {
            ReceiveAny(o => testActor.Forward(o));
        });
    }
}
```

## 2-1. 프로덕션 코드
```csharp
[Fact]
public void Production()
{
    var services = new ServiceCollection();
    services.AddTransient<ChildActor>();
    Sys.UseServiceProvider(services.BuildServiceProvider());

    ActorOf<ParentActor>();

    ExpectNoMsg();
}
```
## 3. 작성 방법 (IChildFactory<T>를 상속받은 MockChildFactory 사용)
- 장점: 테스트에 사용하는 자식 Actor를 원하는 Mock으로 치환할 수 있다.
- 단점: 제품에 작성해야 하는 코드가 증가한다.
```csharp
[Fact]
public void Has_Child_Factory()
{
    var services = new ServiceCollection();
    // MockChildActor에서 사용하는 TestActor 의존성을 선언한다.
    services.AddSingleton<IActorRef>(sp => TestActor);
    services.AddSingleton<IChildFactory<ChildActor>, MockChildFactory>();
    services.AddTransient<ParentActor>();
    Sys.UseServiceProvider(services.BuildServiceProvider());

    ActorOf(Sys.DI().Props<ParentActor>());

    ExpectMsg<string>().Should().Be("Hello, Kid");
}
```
```csharp
class ParentActor : ReceiveActor
{
    public ParentActor(IServiceProvider serviceProvider)
    {
        var childActor = Context.ActorOf(
            serviceProvider.GetRequiredService<IChildFactory<ChildActor>>().GetProps());
        childActor.Tell("Hello, Kid");
    }
}
```
```csharp
public interface IChildFactory<T>
{
    Props GetProps();
}
```
```csharp
class MockChildFactory : IChildFactory<ChildActor>
{
    public MockChildFactory(IActorRef testActor)
    {
        TestActor = testActor;
    }
    public IActorRef TestActor { get; }
    public Props GetProps() =>
         Props.Create(() => new MockChildActor(TestActor));
}
```
```csharp
// MockChildActor는 ReceiveActor를 상속받는다.
class MockChildActor : ReceiveActor
{
    public MockChildActor(IActorRef testActor) => 
        ReceiveAny(o => testActor.Forward(o));
}
```

## 3-1. 프로덕션 코드
```csharp
public void Production()
{
    var services = new ServiceCollection();
    services.AddSingleton<IChildFactory<ChildActor>, ChildFactory>();
    services.AddTransient<ParentActor>();
    Sys.UseServiceProvider(services.BuildServiceProvider());

    ActorOf(Sys.DI().Props<ParentActor>());

    ExpectNoMsg();
}
```
```csharp
class ChildFactory : IChildFactory<ChildActor>
{
    public Props GetProps() =>
         Props.Create(() => new ChildActor());
}
```
```csharp
class ChildActor : ReceiveActor
{
    public ChildActor()
    {
        Receive<string>(_ => { });
    }
}
```
