using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed enemy sheets, palettes, spritemaps and per-enemy artwork catalogs an enemy tile artwork catalog is built from (#633).</summary>
[ArchitectureReview("context-bag", "A record of installed artwork catalogs; its members are data, not services.")]
public sealed record EnemyTileArtworkParts
{
    /// <summary>Sheets.</summary>
    public required IReadOnlyDictionary<EnemyDefinitionId, RoomCharacterAtlas> Sheets { get; init; }

    /// <summary>Palettes.</summary>
    public required IReadOnlyDictionary<EnemyDefinitionId, EnemyPaletteSheet> Palettes { get; init; }

    /// <summary>Crocomire melting.</summary>
    public CrocomireMeltingArtwork? CrocomireMelting { get; init; } = null;

    /// <summary>Spritemaps.</summary>
    public EnemySpritemapCatalog? Spritemaps { get; init; } = null;

    /// <summary>Extended frames.</summary>
    public EnemyExtendedFrameCatalog? ExtendedFrames { get; init; } = null;

    /// <summary>Kraid background.</summary>
    public KraidBackgroundArtwork? KraidBackground { get; init; } = null;

    /// <summary>Kraid colors.</summary>
    public KraidColorCatalog? KraidColors { get; init; } = null;

    /// <summary>Gunship liftoff.</summary>
    public GunshipLiftoffArtworkCatalog? GunshipLiftoff { get; init; } = null;

    /// <summary>Ceres door visual.</summary>
    public CeresDoorVisualCatalog? CeresDoorVisual { get; init; } = null;

    /// <summary>Dma sources.</summary>
    public IReadOnlyDictionary<EnemyDefinitionId, int>? DmaSources { get; init; } = null;

    /// <summary>Projectile spritemaps.</summary>
    public EnemyProjectileSpritemapCatalog? ProjectileSpritemaps { get; init; } = null;

    /// <summary>Magdollite palette cycle.</summary>
    public MagdollitePaletteCycle? MagdollitePaletteCycle { get; init; } = null;

    /// <summary>Work robot palette cycle.</summary>
    public WorkRobotPaletteCycle? WorkRobotPaletteCycle { get; init; } = null;

    /// <summary>Crocomire colors.</summary>
    public CrocomireColorCatalog? CrocomireColors { get; init; } = null;

    /// <summary>Draygon colors.</summary>
    public DraygonColorCatalog? DraygonColors { get; init; } = null;

    /// <summary>Phantoon colors.</summary>
    public PhantoonColorCatalog? PhantoonColors { get; init; } = null;

    /// <summary>Chozo and tube colors.</summary>
    public ChozoAndTubeColorCatalog? ChozoAndTubeColors { get; init; } = null;

    /// <summary>Spore spawn colors.</summary>
    public SporeSpawnColorCatalog? SporeSpawnColors { get; init; } = null;

    /// <summary>Dachora colors.</summary>
    public DachoraColorCatalog? DachoraColors { get; init; } = null;

    /// <summary>Shitroid colors.</summary>
    public ShitroidColorCatalog? ShitroidColors { get; init; } = null;

    /// <summary>Baby metroid cutscene colors.</summary>
    public BabyMetroidCutsceneColorCatalog? BabyMetroidCutsceneColors { get; init; } = null;

    /// <summary>Botwoon colors.</summary>
    public BotwoonColorCatalog? BotwoonColors { get; init; } = null;

    /// <summary>Mother brain death colors.</summary>
    public MotherBrainDeathColorCatalog? MotherBrainDeathColors { get; init; } = null;

    /// <summary>Zebetite colors.</summary>
    public ZebetiteColorCatalog? ZebetiteColors { get; init; } = null;

    /// <summary>Norfair ridley colors.</summary>
    public NorfairRidleyColorCatalog? NorfairRidleyColors { get; init; } = null;

    /// <summary>Tourian statue colors.</summary>
    public TourianStatueColorCatalog? TourianStatueColors { get; init; } = null;

    /// <summary>Phantoon bg2 frames.</summary>
    public PhantoonBg2FrameCatalog? PhantoonBg2Frames { get; init; } = null;

    /// <summary>Draygon bg2 frames.</summary>
    public DraygonBg2FrameCatalog? DraygonBg2Frames { get; init; } = null;

    /// <summary>Mother brain corpse.</summary>
    public RoomCharacterAtlas? MotherBrainCorpse { get; init; } = null;

    /// <summary>Mother brain escape text.</summary>
    public RoomCharacterAtlas? MotherBrainEscapeText { get; init; } = null;

    /// <summary>Mother brain special sprites.</summary>
    public MotherBrainSpecialSpriteArtworkCatalog? MotherBrainSpecialSprites { get; init; } = null;

    /// <summary>Crocomire skeleton.</summary>
    public CrocomireSkeletonArtwork? CrocomireSkeleton { get; init; } = null;

    /// <summary>Crocomire bg2 frames.</summary>
    public CrocomireBg2FrameCatalog? CrocomireBg2Frames { get; init; } = null;

    /// <summary>Torizo instruction vram.</summary>
    public TorizoInstructionVramArtwork? TorizoInstructionVram { get; init; } = null;

    /// <summary>Ceres escape tiles.</summary>
    public CeresEscapeTileArtwork? CeresEscapeTiles { get; init; } = null;

    /// <summary>Ceres escape overlay tilemaps.</summary>
    public CeresEscapeOverlayTilemapCatalog? CeresEscapeOverlayTilemaps { get; init; } = null;

    /// <summary>Auxiliary colors.</summary>
    public EnemyAuxiliaryColorCatalog? AuxiliaryColors { get; init; } = null;

    /// <summary>Mother brain body bg2 frames.</summary>
    public MotherBrainBodyBg2FrameCatalog? MotherBrainBodyBg2Frames { get; init; } = null;
}
