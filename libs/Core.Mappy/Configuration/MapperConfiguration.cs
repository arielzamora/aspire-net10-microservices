using System;
using System.Linq.Expressions;

namespace Core.Mappy.Configuration;

public class MapperConfiguration<TSource, TDestination>
{
    private readonly Dictionary<string, Func<TSource, object>> _mappings = new();

    internal Dictionary<string, Func<TSource, object>> GetMappings() => _mappings;

    public void Map<TProperty>(
      Expression<Func<TDestination, TProperty>> destinationMember,
      Expression<Func<TSource, TProperty>> sourceMember)
    {
        if (sourceMember == null)
            throw new ArgumentNullException(nameof(sourceMember));

        var memberName = GetMemberName(destinationMember);
        var compiledSource = sourceMember.Compile();
        _mappings[memberName] = source => compiledSource(source)!;
    }

    private string GetMemberName<TProperty>(
       Expression<Func<TDestination, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        throw new ArgumentException("Expression must be a member expression", nameof(expression));
    }

}
