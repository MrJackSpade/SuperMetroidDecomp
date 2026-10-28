using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The title screen's VRAM allocations beside its Mode 7 planes (#630).</summary>
internal static class TitleVramLayout
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
