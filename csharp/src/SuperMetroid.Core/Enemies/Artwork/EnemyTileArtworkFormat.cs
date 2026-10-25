
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Host-file geometry for native four-bit enemy tile DMA sheets.</summary>
public static class EnemyTileArtworkFormat
{
    /// <summary>File name of the versioned manifest binding native enemy-definition identities to artwork sheets.</summary>
    public const string ManifestFileName = "enemy-tiles.json";
    /// <summary>Current enemy-tile installation schema revision.</summary>
    public const int Version = 70;
    /// <summary>Stable, source-address-free name for a gunship takeoff character chunk.</summary>
    public static string GunshipLiftoffFileName(int index) =>
        $"gunship-liftoff-{index + 1}-tiles.png";
    /// <summary>All distinct ordinary graphics-set definitions in the pinned retail room states.</summary>
    public const int RetailDefinitionCount = 122;
}
