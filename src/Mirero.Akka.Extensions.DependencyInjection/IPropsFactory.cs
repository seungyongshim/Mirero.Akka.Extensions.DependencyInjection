namespace Microsoft.Extensions.DependencyInjection
{
    using Akka.Actor;

    public interface IPropsFactory<out T> where T : ActorBase
    {
        Props Create();
    }
}




