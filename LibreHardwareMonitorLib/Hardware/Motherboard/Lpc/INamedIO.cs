#nullable enable
namespace LibreHardwareMonitor.Hardware.Motherboard.Lpc;

public interface INamedIO
{
    string?[] FanNames { get; }
    string?[] TemperatureNames { get; }
    string?[] ControlNames { get; }
    string?[] VoltageNames { get; }
}
