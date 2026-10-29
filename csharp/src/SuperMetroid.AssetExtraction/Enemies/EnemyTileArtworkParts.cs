using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed enemy sheets, palettes, spritemaps and per-enemy artwork catalogs an enemy tile artwork catalog is built from (#633).</summary>
[ArchitectureReview("context-bag", "A record of installed artwork catalogs; its members are data, not services.")]
public sealed record EnemyTileArtworkParts
{
    /// <summary>Sheets.</summary>
    public required IReadOnlyDictionary<EnemyDefinitionId, RoomCharacterAtlas> Sheets { get; init; }

    /// <summary>Palettes.</summary>
    public required IReadOnlyDictionary<EnemyDefinitionId, EnemyPaletteSheet> Palettes { get; init; }

    /// <summary>Spritemaps.</summary>
    public EnemySpritemapCatalog? Spritemaps { get; init; } = null;

    /// <summary>Extended frames.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames { get; init; } = null;

    /// <summary>Gunship liftoff.</summary>
    public GunshipLiftoffArtworkCatalog? GunshipLiftoff { get; init; } = null;

    /// <summary>Projectile spritemaps.</summary>
    public EnemyProjectileSpritemapCatalog? ProjectileSpritemaps { get; init; } = null;

    /// <summary>Magdollite palette cycle.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle { get; init; } = null;

    /// <summary>Work robot palette cycle.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle { get; init; } = null;

    /// <summary>Chozo and tube colors.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors { get; init; } = null;

    /// <summary>Dachora colors.</summary>
    public DachoraColorCatalog? DachoraColors { get; init; } = null;

    /// <summary>Auxiliary colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors { get; init; } = null;

    /// <summary>The palettes, extended frame placements and colour catalogs gameplay reads.</summary>
    internal EnemyGameplayArtworkParts Gameplay => new()
    {
        Palettes = Palettes,
        ExtendedFramePlacements = ExtendedFrames?.Placements,
        MagdollitePaletteCycle = MagdollitePaletteCycle,
        WorkRobotPaletteCycle = WorkRobotPaletteCycle,
        ChozoAndTubeColors = ChozoAndTubeColors,
        DachoraColors = DachoraColors,
        AuxiliaryColors = AuxiliaryColors,
    };
}
