using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using Core.Mappy.Configuration;
using Core.Mappy.Interfaces;

namespace Core.Mappy;

public class Mapper : IMapper, IConfigurationProvider
{
    private readonly Dictionary<(Type, Type), object> _configurations = new();

    public IConfigurationProvider ConfigurationProvider => this;

    public void CreateMap<TSource, TDestination>()
    {
        _configurations[(typeof(TSource), typeof(TDestination))]
             = new MapperConfiguration<TSource, TDestination>();
    }

    public void CreateMap<TSource, TDestination>(Action<MapperConfiguration<TSource, TDestination>> configure)
    {
        var config = new MapperConfiguration<TSource, TDestination>();
        configure(config);
        var key = (typeof(TSource), typeof(TDestination));
        _configurations[key] = config;
    }

    public TDestination Map<TDestination>(object source)
    {
        if (source is null)
            return default!;

        var sourceType = source.GetType();
        var destinationType = typeof(TDestination);

        // Handle collections
        if (IsCollectionType(destinationType))
        {
            return MapCollection<TDestination>(source);
        }

        var key = (sourceType, destinationType);
        var destination = Activator.CreateInstance<TDestination>();

        if (_configurations.TryGetValue(key, out var config))
        {
            ApplyCustomMapping(source, destination, config);
        }
        else
        {
            ApplyAutoMapping(source, destination);
        }

        return destination!;
    }

    // ------------- internal helpers used by Map<T> (in-memory) -------------

    private static bool IsCollectionType(Type type)
        => typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);

    private static bool IsSimpleType(Type t)
        => t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(Guid);

    private static bool IsNullableOfSimple(Type t, out Type? underlying)
    {
        underlying = Nullable.GetUnderlyingType(t);
        return underlying is not null && IsSimpleType(underlying);
    }

    private void ApplyCustomMapping<TDestination>(object source, TDestination destination, object config)
    {
        var genericMethod = typeof(Mapper)
            .GetMethod(nameof(ApplyMappings), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(source.GetType(), typeof(TDestination));

        genericMethod.Invoke(this, new[] { source, destination, config });
    }

    private void ApplyAutoMapping<TDestination>(object source, TDestination destination)
    {
        var srcType = source.GetType();
        var dstType = typeof(TDestination);

        foreach (var dstProp in dstType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!dstProp.CanWrite) continue;

            var srcProp = srcType.GetProperty(dstProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (srcProp is null) continue;

            var value = srcProp.GetValue(source);
            dstProp.SetValue(destination, value);
        }
    }

    private TDestination MapCollection<TDestination>(object source)
    {
        var sourceType = source.GetType();
        var destinationType = typeof(TDestination);

        var sourceElementType = sourceType.IsArray ?
            sourceType.GetElementType() :
            sourceType.GetGenericArguments()[0];

        var destElementType = destinationType.IsGenericType ?
            destinationType.GetGenericArguments()[0] :
            destinationType.GetElementType();

        var sourceList = ((IEnumerable)source).Cast<object>().ToList();

        if (destElementType is null)
            throw new InvalidOperationException("Destination element type cannot be null.");

        var destList = (IList)Activator.CreateInstance(typeof(List<>)
            .MakeGenericType(destElementType))!;

        if (sourceElementType is null)
            throw new InvalidOperationException("Source element type cannot be null.");

        foreach (var item in sourceList)
        {
            var mapMethod = typeof(Mapper).GetMethod(nameof(Map), BindingFlags.Public | BindingFlags.Instance)!
                .MakeGenericMethod(destElementType);
            var mapped = mapMethod.Invoke(this, new[] { item });
            destList.Add(mapped!);
        }

        // Convert List<T> to the requested destination collection type if needed
        if (destinationType.IsAssignableFrom(destList.GetType()))
        {
            return (TDestination)destList;
        }

        // Try to construct destination type with IEnumerable<T> constructor
        var ctor = destinationType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(destElementType) });
        if (ctor is not null)
        {
            return (TDestination)ctor.Invoke(new object[] { destList });
        }

        return (TDestination)destList;
    }

    // This method is used by ApplyCustomMapping via reflection
    private void ApplyMappings<TSource, TDestination>(TSource source, TDestination destination, object configObj)
    {
        var config = (MapperConfiguration<TSource, TDestination>)configObj;
        var mappings = config.GetMappings();

        foreach (var (memberName, resolver) in mappings)
        {
            var dstProp = typeof(TDestination).GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (dstProp is null || !dstProp.CanWrite) continue;

            var value = resolver(source);
            dstProp.SetValue(destination, value);
        }
    }

    // ----------------- Expression building for ProjectTo --------------------

    internal LambdaExpression BuildProjectionLambda(Type sourceType, Type destinationType)
    {
        // Prefer record-like constructor; fall back to parameterless + MemberInit
        var ctor = destinationType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        var parameter = Expression.Parameter(sourceType, "src");

        if (ctor is not null && ctor.GetParameters().Length > 0)
        {
            var args = new List<Expression>();
            foreach (var p in ctor.GetParameters())
            {
                var argExpr = BuildMemberProjection(parameter, sourceType, p.Name!, p.ParameterType);

                // Ensure argument type matches parameter type (add conversion when needed)
                if (argExpr.Type != p.ParameterType)
                {
                    if (TryConvert(argExpr, p.ParameterType, out var convertedArg))
                    {
                        argExpr = convertedArg;
                    }
                    else
                    {
                        // As last resort, use default(TParam) to avoid expression errors
                        argExpr = Expression.Default(p.ParameterType);
                    }
                }

                args.Add(argExpr);
            }

            var newExpr = Expression.New(ctor, args);
            return Expression.Lambda(newExpr, parameter);
        }
        else
        {
            // MemberInit path (requires settable properties)
            var newExpr = Expression.New(destinationType);
            var bindings = new List<MemberBinding>();

            foreach (var dstProp in destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!dstProp.CanWrite) continue;
                var valueExpr = BuildMemberProjection(parameter, sourceType, dstProp.Name, dstProp.PropertyType);

                if (valueExpr.Type != dstProp.PropertyType &&
                    TryConvert(valueExpr, dstProp.PropertyType, out var convertedValue))
                {
                    valueExpr = convertedValue;
                }

                var bind = Expression.Bind(dstProp, valueExpr);
                bindings.Add(bind);
            }

            var init = Expression.MemberInit(newExpr, bindings);
            return Expression.Lambda(init, parameter);
        }
    }

    private Expression BuildMemberProjection(ParameterExpression srcParam, Type srcType, string memberName, Type targetType)
    {
        // Try to find source member by name (case-insensitive)
        var srcProp = srcType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (srcProp is null)
        {
            // Not found => default(T)
            return Expression.Default(targetType);
        }

        var srcAccess = Expression.Property(srcParam, srcProp);

        // Handle Nullable<T> targets explicitly (Guid -> Guid?, int -> int?, etc.)
        if (IsNullableOfSimple(targetType, out var uType))
        {
            // If source is underlying type -> convert to Nullable<T>
            if (srcAccess.Type == uType)
            {
                return Expression.Convert(srcAccess, targetType);
            }
            // If source already Nullable<T>, pass as-is
            if (srcAccess.Type == targetType)
            {
                return srcAccess;
            }
            // Try convert to underlying and then to Nullable<T>
            if (TryConvert(srcAccess, uType!, out var toUnderlying))
            {
                return Expression.Convert(toUnderlying, targetType);
            }

            return Expression.Default(targetType);
        }

        // If target is same type or assignable, just use the property
        if (targetType.IsAssignableFrom(srcProp.PropertyType))
        {
            return srcAccess;
        }

        // If collection => project element to destination element
        if (IsEnumerableType(targetType, out var targetElemType) &&
            IsEnumerableType(srcProp.PropertyType, out var sourceElemType))
        {
            // Build element selector: (elem) => <projection to targetElemType>
            var elemParam = Expression.Parameter(sourceElemType, "e");
            var elemSelector = BuildProjectionLambda(sourceElemType, targetElemType);
            // Replace parameter with elemParam
            var replacedBody = new ParameterReplacer(elemSelector.Parameters[0], elemParam)
                .Visit(elemSelector.Body);

            var enumerableSelect = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
                .MakeGenericMethod(sourceElemType, targetElemType);

            var toList = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "ToList" && m.GetParameters().Length == 1)
                .MakeGenericMethod(targetElemType);

            var callSelect = Expression.Call(enumerableSelect, srcAccess, Expression.Lambda(replacedBody!, elemParam));
            var callToList = Expression.Call(toList, callSelect);

            // If target type is not List<T>, try to convert from List<T>
            if (targetType.IsAssignableFrom(callToList.Type))
                return callToList;

            // Try construct target type from IEnumerable<T>
            var ctor = targetType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(targetElemType) });
            if (ctor is not null)
                return Expression.New(ctor, callToList);

            return callToList;
        }

        // If target is a complex type (exclude simple and nullable simple), project recursively
        if (!IsSimpleType(targetType))
        {
            var nestedLambda = BuildProjectionLambda(srcProp.PropertyType, targetType);
            var invoked = Expression.Invoke(nestedLambda, srcAccess);
            return invoked;
        }

        // Try simple conversion
        if (TryConvert(srcAccess, targetType, out var converted))
            return converted;

        // Fallback default
        return Expression.Default(targetType);
    }

    private static bool IsEnumerableType(Type type, out Type elementType)
    {
        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
        {
            var ienum = type.IsArray ? type.GetElementType()! :
                       type.IsGenericType ? type.GetGenericArguments()[0] :
                       typeof(object);
            elementType = ienum;
            return true;
        }
        elementType = typeof(void);
        return false;
    }

    private static bool TryConvert(Expression expr, Type targetType, out Expression converted)
    {
        try
        {
            if (expr.Type == targetType)
            {
                converted = expr;
                return true;
            }
            // Nullable<T> handling
            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying is not null)
            {
                // If expr is already Nullable<T> and assignable
                if (targetType.IsAssignableFrom(expr.Type))
                {
                    converted = expr;
                    return true;
                }
                // Convert to underlying then to Nullable<T>
                if (TryConvert(expr, underlying, out var toUnderlying))
                {
                    converted = Expression.Convert(toUnderlying, targetType);
                    return true;
                }
            }

            // Direct assignable cast
            if (targetType.IsAssignableFrom(expr.Type))
            {
                converted = Expression.Convert(expr, targetType);
                return true;
            }

            // Numeric/string conversions where possible
            converted = Expression.Convert(expr, targetType);
            return true;
        }
        catch
        {
            converted = Expression.Default(targetType);
            return false;
        }
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplacer(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : base.VisitParameter(node);
    }
}
