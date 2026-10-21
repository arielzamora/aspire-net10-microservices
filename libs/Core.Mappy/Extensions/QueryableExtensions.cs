using System.Linq.Expressions;
using Core.Mappy.Interfaces;

namespace Core.Mappy.Extensions;

public static class QueryableExtensions
{
    // Usage parity with AutoMapper: source.ProjectTo<TDestination>(_mapper.ConfigurationProvider)
    public static IQueryable<TDestination> ProjectTo<TDestination>(
        this IQueryable source,
        IConfigurationProvider configurationProvider)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (configurationProvider is null) throw new ArgumentNullException(nameof(configurationProvider));

        var mapper = configurationProvider as Core.Mappy.Mapper
                     ?? throw new ArgumentException("The provided configurationProvider is not compatible with Core.Mappy.", nameof(configurationProvider));

        var sourceType = source.ElementType;
        var destinationType = typeof(TDestination);

        // Build lambda: (TSource src) => <projection to TDestination>
        var lambda = mapper.BuildProjectionLambda(sourceType, destinationType);

        // Build and invoke Queryable.Select(source, lambda)
        var selectMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(sourceType, destinationType);

        var projected = (IQueryable<TDestination>)selectMethod.Invoke(null, new object[] { source, lambda })!;
        return projected;
    }
}