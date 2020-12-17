namespace Mirero.Akka.Extensions.DependencyInjection.Abstractions
{
    using global::Akka.Actor;

    public interface IPropsFactory<out T> where T : ActorBase
    {
        Props Create();
    }
}




