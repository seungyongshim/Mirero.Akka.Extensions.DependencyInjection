namespace Microsoft.Extensions.DependencyInjection
{
    using Akka.Actor;

    public interface IPropsFactory<T> where T : ActorBase
    {
        Props Create();
    }
}




