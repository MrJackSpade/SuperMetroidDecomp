
namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Host-file geometry for four-bit enemy tile sheets.</summary>
public static class EnemyTileArtworkFormat
{
    /// <summary>File name of the versioned manifest binding enemy-definition identities to artwork sheets.</summary>
    public const string ManifestFileName = "enemy-tiles.json";
    /// <summary>
    /// Current enemy-tile installation schema revision; 72 installs no colour document only the import reads, and Kraid's eye
    /// colours by purpose (#1637); 75 installs no boss layout only the import composes from (#1638); 76 does both; 77
    /// installs the enemy frames' display bindings and no enemy composition (#1638); 78 binds no frame no animation shows (#1623);
    /// 79 names each frame for the animation segment or table key that first shows it (#1623).
    /// </summary>
    public const int Version = 79;
    /// <summary>Stable, source-address-free name for a gunship takeoff character chunk.</summary>
    public static string GunshipLiftoffFileName(int index) =>
        $"gunship-liftoff-{index + 1}-tiles.png";
    /// <summary>All distinct ordinary graphics-set definitions in the pinned retail room states.</summary>
    public const int RetailDefinitionCount = 122;
}
