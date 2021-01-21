namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Akka.Actor;
    using Akka.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Mirero.Akka.Extensions.DependencyInjection.Abstractions;

    
    public static class UseAkkaExtensions
    {
        public static IHostBuilder UseAkka(this IHostBuilder host,
                                           Action<ActorSystem> startAction = null,
                                           IEnumerable<string> autoExcludeAssemblies = null)
        {
            return host.ConfigureServices((context, services) =>
            {
                services.AddAkka(startAction, autoExcludeAssemblies);
            });
        }
        internal static IServiceCollection AddAkka(this IServiceCollection services,
                                                 Action<ActorSystem> startAction = null,
                                                 IEnumerable<string> autoExcludeAssemblies = null)
        {
            services.AddSingleton<AkkaHostedServiceStart>(sp => x => startAction?.Invoke(x));
            services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
            services.AddHostedService<AkkaHostedService>();

            var assemblies = GetAssemblies(new []
                {
                    "^Microsoft.*",
                    "^Akka.*",
                    "^System.*",
                    "^xunit.*",
                }.Concat(autoExcludeAssemblies ?? Enumerable.Empty<string>()))
            .ToList();

            services.Scan(sc =>
            {
                sc.FromAssemblies(assemblies)
                  .AddClasses(classes => classes.AssignableTo<ReceiveActor>())
                  .AsSelf()
                  .WithTransientLifetime();
            });

            return services;
        }

        private static IEnumerable<Assembly> GetAssemblies(IEnumerable<string> regexFilters)
        {
            return from path in new[]
                   {
                       Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName),
                       Directory.GetCurrentDirectory()
                   }
                   from ext in new[] { "*.dll", "*.exe" }
                   from file in Directory.GetFiles(path, ext)
                   let fileInfo = new FileInfo(file)
                   where !regexFilters.Any(x => Regex.IsMatch(fileInfo.Name, x))
                   let assembly = TryLoadFrom(fileInfo.FullName)
                   where assembly != null
                   select assembly;

            Assembly TryLoadFrom(string assemblyFile)
            {
                try
                {
                    return Assembly.LoadFrom(assemblyFile);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
