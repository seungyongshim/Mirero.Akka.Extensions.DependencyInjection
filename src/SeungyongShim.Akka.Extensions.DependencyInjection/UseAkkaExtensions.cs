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

    public static class UseAkkaExtensions
    {
        public static IServiceCollection AddAkka(this IServiceCollection services, ActorSystem actorSystem, IEnumerable<string> autoRegistrationTargetAssemblies = null)
        {
            services.AddSingleton<ActorSystem>(sp => actorSystem);
            return services.AddAkkaInternal(autoRegistrationTargetAssemblies);
        }

        public static IServiceCollection AddAkka(this IServiceCollection services,
                                                 IEnumerable<string> autoRegistrationTargetAssemblies = null)
        {
            return services.AddAkkaInternal(autoRegistrationTargetAssemblies);
        }
        public static IServiceCollection AddAkka(this IServiceCollection services,
                                                 string actorSystemName,
                                                 Akka.Configuration.Config actorSystemConfig = null,
                                                 IEnumerable<string> autoRegistrationTargetAssemblies = null)
        {
            services.AddSingleton<ActorSystem>(sp => ActorSystem.Create(actorSystemName, actorSystemConfig)
                                                                .UseDependencyInjectionServiceProvider(sp));

            return services.AddAkkaInternal(autoRegistrationTargetAssemblies);
        }

        private static IServiceCollection AddAkkaInternal(this IServiceCollection services, IEnumerable<string> autoRegistrationTargetAssemblies)
        {
            services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));

            var assemblies = GetAssemblies(new[]
            {
                $"^{Assembly.GetExecutingAssembly().GetName().Name}",
                $"^{Assembly.GetCallingAssembly().GetName().Name}",
            }
            .Concat(autoRegistrationTargetAssemblies ?? new[] { $"^{Assembly.GetCallingAssembly().GetName().Name}" }))
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
                       Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                       Directory.GetCurrentDirectory()
                   }
                   from ext in new[] { "*.dll", "*.exe" }
                   from file in Directory.GetFiles(path, ext)
                   let fileInfo = new FileInfo(file)
                   where !Regex.IsMatch(fileInfo.Name, "^Microsoft.*")
                   where !Regex.IsMatch(fileInfo.Name, "^Akka.*")
                   where !Regex.IsMatch(fileInfo.Name, "^System.*")
                   where !Regex.IsMatch(fileInfo.Name, "^xunit.*")
                   let macthedFileNames = regexFilters.Select(x => Regex.IsMatch(fileInfo.Name, x))
                   let isMachedFileNames = macthedFileNames.Any(x => x == true)
                   where isMachedFileNames
                   let assembly = TryLoadFrom(fileInfo.FullName)
                   where assembly != null
                   
                   select assembly;

            Assembly TryLoadFrom(string assemblyFile)
            {
                try
                {
                    return Assembly.LoadFrom(assemblyFile);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
