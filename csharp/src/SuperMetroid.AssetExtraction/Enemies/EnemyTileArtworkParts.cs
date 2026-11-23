using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed enemy sheets, palettes, spritemaps and per-enemy artwork catalogs an enemy tile artwork catalog is built from (#633).</summary>
public sealed partial record EnemyTileArtworkParts
{
    /// <summary>Sheets.</summary>
    public required IReadOnlyDictionary<EnemyDefinitionId, RoomCharacterAtlas> Sheets { get; init; }

    /// <summary>Palettes.</summary>
    public required IReadOnlyDictionary<EnemyDefinitionId, EnemyPaletteSheet> Palettes { get; init; }

    /// <summary>The characters of the artwork graphics sets load for no enemy (#1686).</summary>
    internal IReadOnlyDictionary<UnplacedEnemyArtwork, RoomCharacterAtlas> UnplacedSheets { get; init; } = new Dictionary<UnplacedEnemyArtwork, RoomCharacterAtlas>();

    /// <summary>The colours of the artwork graphics sets load for no enemy (#1686).</summary>
    internal IReadOnlyDictionary<UnplacedEnemyArtwork, EnemyPaletteSheet> UnplacedPalettes { get; init; } = new Dictionary<UnplacedEnemyArtwork, EnemyPaletteSheet>();

    /// <summary>Spritemaps.</summary>
    public required EnemySpritemapCatalog Spritemaps { get; init; }

    /// <summary>Extended frames.</summary>
    public required EnemyExtendedFrameCatalog ExtendedFrames { get; init; }

    /// <summary>Gunship liftoff.</summary>
    public required GunshipLiftoffArtworkCatalog GunshipLiftoff { get; init; }

    /// <summary>Projectile spritemaps.</summary>
    public required EnemyProjectileSpritemapCatalog ProjectileSpritemaps { get; init; }

    /// <summary>Magdollite palette cycle.</summary>
    public required MagdollitePaletteCycle MagdollitePaletteCycle { get; init; }

    /// <summary>Work robot palette cycle.</summary>
    public required WorkRobotPaletteCycle WorkRobotPaletteCycle { get; init; }

    /// <summary>Chozo and tube colors.</summary>
    public required ChozoAndTubeColorCatalog ChozoAndTubeColors { get; init; }

    /// <summary>Dachora colors.</summary>
    public required DachoraColorCatalog DachoraColors { get; init; }

    /// <summary>Auxiliary colors.</summary>
    public required EnemyAuxiliaryColorCatalog AuxiliaryColors { get; init; }
}
