using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The WRAM word Mother Brain's grey palette copies spill into (#629).</summary>
[CpuBusBoundary("catalog: Mother Brain palette-copy WRAM spill")]
public static class MotherBrainPaletteWorkRam
{
    /// <summary>
    /// $7E:017C: $AD:EF44/EF81 write the grey transition's trailing source word directly to this
    /// low-WRAM word rather than to CGRAM.
    /// </summary>
    public static readonly CpuBusCell GreyTrailingWord = CpuBusCell.WramWord(nameof(GreyTrailingWord), 0x7e017c,
        "Bgr555 (u16)");
}

/// <summary>Typed view of the WRAM word Mother Brain's grey palette copies spill into (#629).</summary>
[CpuBusBoundary("domain view: Mother Brain palette-copy WRAM spill")]
public readonly struct MotherBrainPaletteMemory(ISnesMemoryBus bus)
{
    /// <summary>The grey transition's trailing color, stored low byte first.</summary>
    public Bgr555 GreyTrailingWord
    {
        set => new WramWord(bus, MotherBrainPaletteWorkRam.GreyTrailingWord).Write(value.ToWord());
    }
}
