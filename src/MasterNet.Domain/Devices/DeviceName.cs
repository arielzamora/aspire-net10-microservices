namespace MasterNet.Domain.Devices;
public class DeviceName : IEquatable<DeviceName>
{
    public string Value { get; }

    public DeviceName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Empty", nameof(value));
        }

        Value = value;
    }

    public bool Equals(DeviceName? other)
      => other is not null && StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is DeviceName o && Equals(o);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
}
