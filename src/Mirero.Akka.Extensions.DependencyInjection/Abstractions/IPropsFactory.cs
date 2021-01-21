using Akka.Actor;

namespace Mirero.Akka.Extensions.DependencyInjection.Abstractions
{
    public interface IPropsFactory<out T> where T : ActorBase
    {
        Props Create(params object[] args);
    }
}




