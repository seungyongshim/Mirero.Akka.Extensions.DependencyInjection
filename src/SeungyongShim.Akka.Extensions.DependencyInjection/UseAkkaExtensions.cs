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
                $"^{Assembly.GetExecutingAssembly().GetName().Name}$",
                $"^{Assembly.GetCallingAssembly().GetName().Name}$",
            }
            .Concat(autoRegistrationTargetAssemblies ?? new[] { "" }))
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
                   let assembly = TryLoadFrom(file)
                   where assembly != null
                   where !Regex.IsMatch(assembly.FullName, "^Microsoft.*")
                   where !Regex.IsMatch(assembly.FullName, "^Akka.*")
                   where !Regex.IsMatch(assembly.FullName, "^System.*")
                   where !Regex.IsMatch(assembly.FullName, "^xunit.*")
                   where regexFilters.Where(x => Regex.IsMatch(assembly.GetName().Name, x))
                                     .Any()
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
