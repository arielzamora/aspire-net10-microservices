using MasterNet.Domain.Abstractions;
using MasterNet.Domain.Devices;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MasterNet.Persistence.Specifications;
public sealed class DeviceByNameContainsSpec : ISpecification<Device>
{
    public Expression<Func<Device, bool>> Criteria { get;}

    public DeviceByNameContainsSpec(string? term)
    {
        term = (term ?? string.Empty).Trim();
        Criteria = d => EF.Functions.Like(d.DeviceName.Value, $"%{term}%");
    }
}
