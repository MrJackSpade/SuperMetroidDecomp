namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge WRAM mirrors written to COLDATA during NMI (#629).</summary>
[CpuBusBoundary("catalog: COLDATA fixed-color WRAM mirrors")]
public static class PpuFixedColorMirrors
{
    /// <summary>$00:0074, DP_ColorMathSubScreenBackdropColor0, normally the red write.</summary>
    public static readonly CpuBusCell Red = CpuBusCell.WramByte(nameof(Red), 0x000074, "COLDATA write (u8: channel bits | 5-bit intensity)");

    /// <summary>$00:0075, DP_ColorMathSubScreenBackdropColor1; Fireflea deliberately writes blue here.</summary>
    public static readonly CpuBusCell Green = CpuBusCell.WramByte(nameof(Green), 0x000075, "COLDATA write (u8: channel bits | 5-bit intensity)");

    /// <summary>$00:0076, DP_ColorMathSubScreenBackdropColor2; Fireflea deliberately writes green here.</summary>
    public static readonly CpuBusCell Blue = CpuBusCell.WramByte(nameof(Blue), 0x000076, "COLDATA write (u8: channel bits | 5-bit intensity)");
}

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
