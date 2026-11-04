using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts only charge-flare sprite parts, not animation or gameplay data.</summary>
public static class ChargeFlareSpriteExtractor
{
    /// <summary>Installed JSON resource filename for charge/Hyper/grapple flare and spark sprite compositions, separate from ordinary projectile compositions.</summary>
    public const string FileName = "charge-flare-compositions.json";

    /// <summary>Extracts the cataloged native charge-flare spritemap parts.</summary>
    /// <param name="bus">Supported-cartridge address space containing the charge-flare spritemaps.</param>
    /// <returns>UTF-8 JSON bytes containing charge-flare sprite parts only.</returns>
    public static byte[] Extract(ISnesAddressSpace bus) =>
        ProjectileSpriteExtractor.ExtractFrames(bus, ChargeFlareSpriteDefinitions.All, ProjectileSpriteLocations.Flares());
}
