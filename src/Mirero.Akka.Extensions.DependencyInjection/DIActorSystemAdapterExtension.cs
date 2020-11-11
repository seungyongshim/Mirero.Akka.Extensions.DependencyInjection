namespace Akka.DI.Core
{
    using System.Reflection;
    using Akka.Actor;
    using Mirero.Akka.Extensions.DependencyInjection;
    using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

    public static class DIActorSystemAdapterExtension
    {
        public static IPropsFactory<T> PropsFactory<T>(this DIActorContextAdapter context) where T : ActorBase =>
            context.GetFieldValue<IActorContext>("context").System.GetExtension<DIExt2>().PropsFactory<T>();

        public static IPropsFactory<T> PropsFactory<T>(this DIActorSystemAdapter context) where T : ActorBase =>
            context.GetFieldValue<ActorSystem>("system").GetExtension<DIExt2>().PropsFactory<T>();
    }

    internal static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
    }
}
