using System.Linq.Expressions;

namespace MasterNet.Domain.Abstractions;
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}
