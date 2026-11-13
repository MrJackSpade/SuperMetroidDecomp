using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Extracts the compositions of the burst Samus's hit shows at an enemy it hurts (#1713), one of her weapon effects, whose
/// pictures her projectile sheets hold.
/// </summary>
public static class HitBurstSpriteExtractor
{
    /// <summary>Import-data filename for the burst's compositions, separate from the projectile and flare compositions.</summary>
    public const string FileName = "hit-burst-compositions.json";

    /// <summary>Extracts the burst's four spritemaps' parts.</summary>
    /// <param name="bus">Supported-cartridge address space containing the burst's spritemaps.</param>
    /// <returns>UTF-8 JSON bytes containing the burst's sprite parts only.</returns>
    public static byte[] Extract(ISnesAddressSpace bus) =>
        ProjectileSpriteExtractor.ExtractFrames(bus, HitBurstSpriteDefinitions.All, ProjectileSpriteLocations.HitBursts());
}
