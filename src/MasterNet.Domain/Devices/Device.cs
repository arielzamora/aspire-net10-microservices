using MasterNet.Domain.Abstractions;

namespace MasterNet.Domain.Devices;
public sealed class Device : BaseEntity
{
    public DeviceName DeviceName { get; private set; }

    private Device() { }

    public Device(DeviceName deviceName)
    {
        DeviceName = deviceName;
    }
}
