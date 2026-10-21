using System.Reflection;
using Core.MediatOR;
using Core.MediatOR.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Core.MediatOR;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatOR(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediatOR, MediatOR>();

        foreach (var assembly in assemblies)
        {
            // Includes public, internal, and nested types
            var types = assembly.GetTypes();

            foreach (var type in types.Where(t => t.IsClass && !t.IsAbstract))
            {
                var handlerInterfaces = type
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    .ToArray();

                if (handlerInterfaces.Length == 0) continue;

                if (type.IsGenericTypeDefinition)
                {
                    // Open-generic handler (rare): register as open-generic
                    services.AddScoped(typeof(IRequestHandler<,>), type);
                }
                else
                {
                    // Closed handlers: register each closed interface to the implementation
                    foreach (var itf in handlerInterfaces)
                    {
                        services.AddScoped(itf, type);
                    }
                }
            }
        }

        return services;
    }
}
