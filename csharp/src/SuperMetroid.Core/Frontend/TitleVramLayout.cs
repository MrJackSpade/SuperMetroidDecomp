using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>The title screen's VRAM allocations beside its Mode 7 planes (#630).</summary>
[PpuLayoutBoundary("catalog: title screen VRAM")]
public static class TitleVramLayout
{
    /// <summary>VMADD $6000-$7FFF (bytes $C000-$FFFF): the title OBJ character sheet.</summary>
    public static readonly PpuLayoutCell ObjectCharacters = new(nameof(ObjectCharacters), PpuLayoutRegion.Vram,
        0x6000, 0x2000, "4-bpp OBJ characters", PpuMemoryLayout.Vram);

    /// <summary>
    /// VMADD $3800-$38FF high bytes: Mode 7 characters $E0-$E3, which the Baby Metroid animation
    /// (<c>$8B:A131</c> page cycle) replaces one $100-byte page at a time.
    /// </summary>
    public static readonly PpuLayoutCell BabyMetroidCharacters = new(nameof(BabyMetroidCharacters), PpuLayoutRegion.Vram,
        0x3800, 0x0100, "Mode 7 8-bpp pixels (u8, VRAM high bytes)", Mode7VramLayout.Characters);
}

/// <summary>Typed view of the title screen's VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: title screen VRAM")]
public readonly struct TitleVram(SnesVram vram)
{
    /// <summary>The title OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, TitleVramLayout.ObjectCharacters);

    /// <summary>Writes one Baby Metroid character page through the Mode 7 high-byte port, keeping the map bytes.</summary>
    public void LoadBabyMetroidPage(ReadOnlySpan<byte> pixels)
    {
        Ensure.LengthEqual(pixels, TitleVramLayout.BabyMetroidCharacters.Count);
        new Mode7Vram(vram).LoadCharacters(pixels, TitleVramLayout.BabyMetroidCharacters.Offset);
    }
}
