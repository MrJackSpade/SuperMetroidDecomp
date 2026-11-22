using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The ending's VRAM allocations (#630): the escape/explosion OBJ sheets, the credits font and
/// tilemap, and the post-credits waiting, shooting and reward scenes. Each scene reuses VRAM, so
/// its allocations alias the whole store.
/// </summary>
[EncodingOwner("ending VRAM allocations")]
internal static partial class EndingVramLayout
{
    /// <summary>VMADD $4000-$6FFF: the escape/explosion OBJ sheet <c>$8B:D8C1</c> uploads from <c>$7F:8000</c>.</summary>
    public static readonly PpuLayoutCell ExplosionObjects = Vram(nameof(ExplosionObjects), 0x4000, 0x3000, "4-bpp OBJ characters");

    /// <summary>VMADD $5000-$57FF: the ending font OBJ characters loaded after the explosion sheet.</summary>
    public static readonly PpuLayoutCell EndingFont = Vram(nameof(EndingFont), 0x5000, 0x0800, "4-bpp OBJ characters");

    /// <summary>VMADD $5000-$5FFF: the waiting-Samus BG characters of the post-credits scene.</summary>
    public static readonly PpuLayoutCell WaitingCharacters = Vram(nameof(WaitingCharacters), 0x5000, 0x1000, "4-bpp BG characters");

    /// <summary>VMADD $4C00-$4FFF: the waiting-Samus BG tilemap.</summary>
    public static readonly PpuLayoutCell WaitingTilemap = Vram(nameof(WaitingTilemap), 0x4c00, 0x0400, "BG tilemap words (32x32)");

    /// <summary>VMADD $6000-$7FFF: the escape cloud and post-credits shooting-scene OBJ characters.</summary>
    public static readonly PpuLayoutCell PostCreditsObjects = Vram(nameof(PostCreditsObjects), 0x6000, 0x2000, "4-bpp OBJ characters");

    /// <summary>VMADD $7000-$73FF: the wide explosion's characters.</summary>
    public static readonly PpuLayoutCell WideExplosionCharacters = Vram(nameof(WideExplosionCharacters), 0x7000, 0x0400, "4-bpp OBJ characters");

    /// <summary>VMADD $7400-$77FF: the concentric explosion's characters.</summary>
    public static readonly PpuLayoutCell ConcentricExplosionCharacters = Vram(nameof(ConcentricExplosionCharacters), 0x7400, 0x0400, "4-bpp OBJ characters");

    /// <summary>VMADD $7800-$7BFF: the eclipse's characters.</summary>
    public static readonly PpuLayoutCell EclipseCharacters = Vram(nameof(EclipseCharacters), 0x7800, 0x0400, "4-bpp OBJ characters");

    /// <summary>VMADD $7C00-$7FFF: the burst glow's characters.</summary>
    public static readonly PpuLayoutCell BurstGlowCharacters = Vram(nameof(BurstGlowCharacters), 0x7c00, 0x0400, "4-bpp OBJ characters");

    /// <summary>VMADD $0000-$1FFF: the post-credits reward characters (suitless sheet, or the reused waiting sheet).</summary>
    public static readonly PpuLayoutCell RewardCharacters = Vram(nameof(RewardCharacters), 0x0000, 0x2000, "4-bpp BG characters");

    /// <summary>VMADD $2000-$207F: the post-credits transformation's characters.</summary>
    public static readonly PpuLayoutCell TransformationCharacters = Vram(nameof(TransformationCharacters), 0x2000, 0x0080, "4-bpp characters");

    /// <summary>VMADD $2400-$27FF: the post-credits transformation's map.</summary>
    public static readonly PpuLayoutCell TransformationMap = Vram(nameof(TransformationMap), 0x2400, 0x0400, "4-bpp characters");

    /// <summary>Declares a native VRAM word interval against the full store for ending-scene overlap checks.</summary>
    private static PpuLayoutCell Vram(string name, int word, int count, string encoding) =>
        new(name, PpuLayoutRegion.Vram, word, count, encoding, PpuMemoryLayout.Vram);
}
