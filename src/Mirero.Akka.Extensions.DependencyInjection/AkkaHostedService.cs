namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Akka.Actor;
    using Akka.Util.Internal;
    using Microsoft.Extensions.Hosting;

    internal class AkkaHostedService : IHostedService
    {
        public AkkaHostedService(IServiceProvider serviceProvider, ActorSystem actorSystem)
        {
            ServiceProvider = serviceProvider;
            ActorSystem = actorSystem;
        }

        public IServiceProvider ServiceProvider { get; }
        public ActorSystem ActorSystem { get; }

        public async Task StartAsync(CancellationToken cancellationToken) => await Task.CompletedTask;

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
