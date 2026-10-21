using Core.Mappy.Configuration;

namespace Core.Mappy.Interfaces;

public interface IMapper
{
    // Mapping configuration
    void CreateMap<TSource, TDestination>();
    void CreateMap<TSource, TDestination>(Action<MapperConfiguration<TSource, TDestination>> configure);

    // Object mapping
    TDestination Map<TDestination>(object source);

    // Expose a configuration provider so callers can pass it to ProjectTo(...)
    IConfigurationProvider ConfigurationProvider { get; }
}
