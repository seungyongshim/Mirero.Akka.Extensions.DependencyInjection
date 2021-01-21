using Akka.Actor;

namespace Mirero.Akka.Extensions.DependencyInjection
{
    public interface IActorRef<T> : IActorRef where T : ActorBase
    {
    }
}
