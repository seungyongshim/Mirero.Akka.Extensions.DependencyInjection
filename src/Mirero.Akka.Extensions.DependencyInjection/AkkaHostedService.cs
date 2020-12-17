namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Akka.Actor;
    using Akka.Util.Internal;
    using Microsoft.Extensions.Hosting;

    internal delegate void AkkaHostedServiceStart(ActorSystem actorSystem);

    internal class AkkaHostedService : IHostedService
    {
        public AkkaHostedService(IServiceProvider serviceProvider, ActorSystem actorSystem, AkkaHostedServiceStart akkaHostedServiceStart)
        {
            ServiceProvider = serviceProvider;
            ActorSystem = actorSystem;
            AkkaHostedServiceStart = akkaHostedServiceStart;
        }

        public IServiceProvider ServiceProvider { get; }
        public ActorSystem ActorSystem { get; }
        public AkkaHostedServiceStart AkkaHostedServiceStart { get; }

        public async Task StartAsync(CancellationToken cancellationToken) 
        {
            AkkaHostedServiceStart(ActorSystem);
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                ActorSystem.Terminate().Wait(cts.Token);
            }

            return Task.CompletedTask;
        }
    }
}
