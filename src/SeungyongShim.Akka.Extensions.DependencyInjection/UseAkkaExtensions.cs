namespace Microsoft.Extensions.DependencyInjection
{
    using Akka.Actor;

    public static class UseAkkaExtensions
    {
        public static IServiceCollection AddAkka(this IServiceCollection services, ActorSystem actorSystem)
        {
            services.AddSingleton<ActorSystem>(sp => actorSystem);
            services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));

            return services;
        }

        public static IServiceCollection AddAkka(this IServiceCollection services, string actorSystemName, Akka.Configuration.Config actorSystemConfig = null)
        {
            var actorSystem = ActorSystem.Create(actorSystemName, actorSystemConfig);

            return services.AddAkka(actorSystem);
        }
    }
}
