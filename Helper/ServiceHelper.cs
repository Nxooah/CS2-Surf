using CSurf.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSurf.Helper
{
    public static class ServiceHelper
    {

        private static readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();

        public static void AddAllTypes<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where T : class
        {
            #region T is interface

            var typesOfInterface = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.DefinedTypes)
                .Where(x => x.IsClass && !x.IsAbstract && x.GetInterfaces().Contains(typeof(T)));

            foreach (var type in typesOfInterface)
            {
                Console.WriteLine($"Registering {type.Name} (implements interface {typeof(T).Name}) with lifetime {Enum.GetName(lifetime)}");

                if (services.Any(e => e.ServiceType == type))
                {
                    Console.WriteLine($"Skipping registration of {type.Name} -> already registered!");
                    continue;
                }

                if (type.ImplementedInterfaces.Contains(typeof(CSurfModule)))
                {
                    servicesToInstanciate.Add(type);
                    lifetime = ServiceLifetime.Singleton;

                    Console.WriteLine($"Configured {type.Name} for instanciation on startup");
                }

                // add as resolvable by implementation type
                services.Add(new ServiceDescriptor(type, type, lifetime));

                if (typeof(T) != type)
                {
                    // add as resolvable by service type (forwarding)
                    services.Add(new ServiceDescriptor(typeof(T), x => x.GetRequiredService(type), lifetime));
                }
            }

            #endregion

            #region T is class

            var typesOfClasses = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (var type in typesOfClasses)
            {
                Console.WriteLine($"Registering {type.Name} (inherits class {typeof(T).Name}) with lifetime {Enum.GetName(lifetime)}");

                if (services.Any(e => e.ServiceType == type))
                {
                    Console.WriteLine($"Skipping registration of {type.Name} -> already registered!");
                    continue;
                }
                if (type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(CSurfModule)))
                {
                    servicesToInstanciate.Add(type);
                    lifetime = ServiceLifetime.Singleton;

                    Console.WriteLine($"Configured {type.Name} for instanciation on startup");
                }

                // add as resolvable by implementation type
                services.Add(new ServiceDescriptor(type, type, lifetime));

                if (typeof(T) != type)
                {
                    // add as resolvable by service type (forwarding)
                    services.Add(new ServiceDescriptor(typeof(T), x => x.GetRequiredService(type), lifetime));
                }
            }

            #endregion
        }

        private static object GetSingletonInstance(IServiceProvider serviceProvider, Type type)
        {
            if (_singletonInstances.ContainsKey(type))
            {
                return _singletonInstances[type];
            }

            var instance = serviceProvider.GetRequiredService(type);
            _singletonInstances.Add(type, instance);

            return instance;
        }

        private static readonly List<Type> servicesToInstanciate = new List<Type>();

        public static void InstanciateStartupScripts(this ServiceProvider provider)
        {
            Console.WriteLine("Dependency Injection: Instanciating registered scripts");

            foreach (var type in servicesToInstanciate)
            {
                _ = provider.GetService(type);

                Console.WriteLine($"Instanciated {type.Name}");
            }
        }
    }
}
