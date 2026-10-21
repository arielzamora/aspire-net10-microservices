using Microsoft.Extensions.DependencyInjection;
using Core.Mappy.Extensions;
using Core.MediatOR;
using Core.MediatOR.Contracts; // IPipelineBehavior
using MasterNet.Application.Core;
using Core.MediatOR; // ValidationBehavior

namespace MasterNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services
    )
    {
        services.AddMediatOR(typeof(DependencyInjection).Assembly);

        // Register the open-generic pipeline behavior explicitly
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register all IValidator<T> from this assembly (internal/public)
        var asm = typeof(DependencyInjection).Assembly;
        foreach (var type in asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            var validatorInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .ToArray();

            foreach (var itf in validatorInterfaces)
            {
                services.AddScoped(itf, type);
            }
        }


        services.AddMapper();
        return services;
    }
}