using System;
using System.Reflection;
using Core.Mappy.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Mappy.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddMapper(this IServiceCollection services)
    {
        services.AddSingleton<IMapper, Mapper>();
        return services;
    }

    public static IMapper RegisterMappings(
        this IMapper mapper,
        Assembly assembly
    )
    {
        var mappingProfiles = assembly.GetTypes()
            .Where(t =>
                typeof(IMappingProfile).IsAssignableFrom(t) &&
                t.IsClass &&
                !t.IsAbstract);

        foreach (var profileType in mappingProfiles)
        {
            // Crear instancia del perfil
            var profileInstance = Activator.CreateInstance(profileType);
            if (profileInstance is IMappingProfile mappingProfile)
            {
                // Ejecutar Configure sobre la instancia
                mappingProfile.Configure(mapper);
            }
        }

        return mapper;
    }

}
