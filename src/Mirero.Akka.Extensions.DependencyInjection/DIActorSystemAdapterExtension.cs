namespace Microsoft.Extensions.DependencyInjection
{
    using Akka.Actor;
    using Akka.DI.Core;
    using SeungyongShim.Akka.Extensions.DependencyInjection;
    using SeungyongShim.Akka.Extensions.DependencyInjection.Extensions;

    public static class DIActorSystemAdapterExtension
    {
        public static IPropsFactory<T> PropsFactory<T>(this DIActorContextAdapter context) where T : ActorBase =>
            context.GetFieldValue<IActorContext>("context").System.GetExtension<DIExt2>().PropsFactory<T>();


        public static IPropsFactory<T> PropsFactory<T>(this DIActorSystemAdapter context) where T : ActorBase =>
            context.GetFieldValue<ActorSystem>("system").GetExtension<DIExt2>().PropsFactory<T>();
        
    }
}
