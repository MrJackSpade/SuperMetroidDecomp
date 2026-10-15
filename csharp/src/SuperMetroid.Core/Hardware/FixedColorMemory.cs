namespace SuperMetroid.Core.Hardware;

/// <summary>Typed view of the three COLDATA fixed-color mirrors (#629).</summary>
[CpuBusBoundary("domain view: COLDATA fixed-color WRAM mirrors")]
public readonly struct FixedColorMemory(ISnesMemoryBus bus)
{
    /// <summary>The raw COLDATA byte mirrored at <see cref="PpuFixedColorMirrorAddresses.Red"/>.</summary>
    public byte Red
    {
        get => new WramByte(bus, PpuFixedColorMirrorAddresses.Red).Read();
        set => new WramByte(bus, PpuFixedColorMirrorAddresses.Red).Write(value);
    }

    /// <summary>The raw COLDATA byte mirrored at <see cref="PpuFixedColorMirrorAddresses.Green"/>.</summary>
    public byte Green
    {
        get => new WramByte(bus, PpuFixedColorMirrorAddresses.Green).Read();
        set => new WramByte(bus, PpuFixedColorMirrorAddresses.Green).Write(value);
    }

    /// <summary>The raw COLDATA byte mirrored at <see cref="PpuFixedColorMirrorAddresses.Blue"/>.</summary>
    public byte Blue
    {
        get => new WramByte(bus, PpuFixedColorMirrorAddresses.Blue).Read();
        set => new WramByte(bus, PpuFixedColorMirrorAddresses.Blue).Write(value);
    }
}
