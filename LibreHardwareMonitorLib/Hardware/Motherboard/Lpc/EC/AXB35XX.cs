using System;

namespace LibreHardwareMonitor.Hardware.Motherboard.Lpc.EC;

internal class AXB35XX : ISuperIO, INamedIO
{
    public Chip Chip => Chip.AXB35;

    public AXB35XX()
    {
        Fans = new float?[3];
        Controls = new float?[3];
        Temperatures = new float?[1];
        Voltages = Array.Empty<float?>();

        FanNames = new[] { "Fan 1", "Fan 2", "System Fan" };
        ControlNames = new[] { "Fan 1", "Fan 2", "System Fan" };
        TemperatureNames = new[] { "CPU" };
        VoltageNames = Array.Empty<string>();
    }

    public float?[] Controls { get; }
    public float?[] Fans { get; }
    public float?[] Temperatures { get; }
    public float?[] Voltages { get; }

    private static readonly ushort[] _registers =
    {
        0x21,       // FAN1CONTROL_MODE
        0x22,       // FAN1CONTROL_VALUE
        0x23,       // FAN2CONTROL_MODE
        0x24,       // FAN2CONTROL_VALUE
        0x25,       // SYSFAN2CONTROL_MODE
        0x26,       // SYSFAN2CONTROL_VALUE
        0x28, 0x29, // SYSFAN
        0x35, 0x36, // FAN1
        0x37, 0x38, // FAN2
        0x70,       // CPU_TEMP
        //0x31,     // POWER_MODE
    };

    private bool _firstRead = true;

    public void Update()
    {
        if (_firstRead)
        {
            _firstRead = false;
            return; // helps with long initializing in FanControl
        }

        byte[] data = new byte[_registers.Length];
        WindowsEmbeddedControllerIO.Instance.Read(_registers, data);

        Controls[0] = GetFanControlValue(data[0], data[1]);
        Controls[1] = GetFanControlValue(data[2], data[3]);
        Controls[2] = GetFanControlValue(data[4], data[5]);

        Fans[0] = GetRpm(data[8], data[9]);
        Fans[1] = GetRpm(data[10], data[11]);
        int rpm = GetRpm(data[6], data[7]);
        Fans[2] = rpm == 8000 ? null : rpm;

        Temperatures[0] = data[12];
    }

    static float? GetFanControlValue(byte mode, byte value)
    {
        if ((mode & 0x1) == 0)
        {
            // AUTO
            return null;
        }

        return (value & 0xF) switch
        {
            0x2 => 20,
            0x3 => 40,
            0x4 => 60,
            0x5 => 80,
            0x6 => 100,
            _ => 0
        };
    }

    static int GetRpm(byte hi, byte lo)
    {
        return (hi << 8) | lo;
    }


    public void SetControl(int index, byte? value)
    {
        const float ratio = 255f / 5;   // 255 = 100% = level 5

        byte valBase = (byte)((index + 1) << 4);

        if (value == null)
        {
            // Set auto
            WindowsEmbeddedControllerIO.Instance.WriteSingle((byte)_registers[index * 2], valBase, 1000);
        }
        else
        {
            byte[] regs = { (byte)_registers[index * 2], (byte)_registers[index * 2 + 1] };
            byte[] vals = { (byte)(valBase + 1), (byte)(valBase + (value == 0 ? 7 : Math.Round(value.Value / ratio) + 1)) };
            WindowsEmbeddedControllerIO.Instance.Write(regs, vals, 1000);
        }
    }

    public string[] FanNames { get; }
    public string[] TemperatureNames { get; }
    public string[] ControlNames { get; }
    public string[] VoltageNames { get; }

    public byte? ReadGpio(int index)
    {
        return null;
    }

    public void WriteGpio(int index, byte value)
    {

    }

    public string GetReport()
    {
        return "TODO";
    }
}
