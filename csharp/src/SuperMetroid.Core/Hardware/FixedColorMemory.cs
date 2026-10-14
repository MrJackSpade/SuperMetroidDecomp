namespace SuperMetroid.Core.Hardware;

/// <summary>Typed view of the three COLDATA fixed-color mirrors (#629).</summary>
[CpuBusBoundary("domain view: COLDATA fixed-color WRAM mirrors")]
public readonly struct FixedColorMemory(ISnesMemoryBus bus)
{
    /// <summary>The raw COLDATA byte mirrored at <see cref="PpuFixedColorMirrors.Red"/>.</summary>
    public byte Red
    {
        get => new WramByte(bus, PpuFixedColorMirrors.Red).Read();
        set => new WramByte(bus, PpuFixedColorMirrors.Red).Write(value);
    }

    /// <summary>The raw COLDATA byte mirrored at <see cref="PpuFixedColorMirrors.Green"/>.</summary>
    public byte Green
    {
        get => new WramByte(bus, PpuFixedColorMirrors.Green).Read();
        set => new WramByte(bus, PpuFixedColorMirrors.Green).Write(value);
    }

    /// <summary>The raw COLDATA byte mirrored at <see cref="PpuFixedColorMirrors.Blue"/>.</summary>
    public byte Blue
    {
        get => new WramByte(bus, PpuFixedColorMirrors.Blue).Read();
        set => new WramByte(bus, PpuFixedColorMirrors.Blue).Write(value);
    }

    /// <summary>The five-bit intensity of <see cref="Red"/>.</summary>
    public byte RedIntensity => (byte)(Red & 31);

    /// <summary>The five-bit intensity of <see cref="Green"/>.</summary>
    public byte GreenIntensity => (byte)(Green & 31);

    /// <summary>The five-bit intensity of <see cref="Blue"/>.</summary>
    public byte BlueIntensity => (byte)(Blue & 31);
}
