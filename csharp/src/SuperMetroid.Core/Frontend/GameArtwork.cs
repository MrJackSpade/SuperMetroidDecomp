using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Every installed presentation catalog the frontend binds: maps, room and PLM art, cinematic, ending, Samus,
/// enemy and projectile artwork. Hosts load one set from an installation and bind it with
/// <see cref="SuperMetroidGame.BindArtwork"/>; a restored debugger state rebinds the same set.
/// </summary>
public sealed record GameArtwork
{
    /// <summary>No catalog bound: the state of a new game, or of a restored one before its host rebinds.</summary>
    public static GameArtwork Unbound { get; } = new();

    /// <summary>Area maps with the pause, HUD, title, intro and ending text and palettes that share their installation.</summary>
    public AreaMapPresentationCatalog? MapPresentation { get; init; }

    /// <summary>Gameplay base palettes a new runtime starts from.</summary>
    public GameplayBasePaletteCatalog? GameplayBasePalettes { get; init; }

    /// <summary>Common gameplay sprite characters (standard objects).</summary>
    public RoomCharacterAtlas? StandardObjectArt { get; init; }

    /// <summary>Intro, Ceres destruction and Ceres flight artwork.</summary>
    public IntroCinematicArtworkCatalog? IntroCinematicArt { get; init; }

    /// <summary>Samus body tiles, palettes and death palettes.</summary>
    public SamusBodyArtworkCatalog? SamusBodyArt { get; init; }

    /// <summary>Ending Mode 7 artwork.</summary>
    public EndingMode7ArtworkCatalog? EndingMode7Art { get; init; }

    /// <summary>Ending sprite artwork.</summary>
    public EndingObjectArtworkCatalog? EndingObjectArt { get; init; }

    /// <summary>Ending palettes.</summary>
    public EndingPaletteCatalog? EndingPaletteArt { get; init; }

    /// <summary>Room tileset characters.</summary>
    public RoomCharacterAtlasCatalog? RoomCharacterArt { get; init; }

    /// <summary>Room static palettes.</summary>
    public RoomStaticPaletteCatalog? RoomPaletteArt { get; init; }

    /// <summary>Room metatiles.</summary>
    public RoomMetatileCatalog? RoomMetatileArt { get; init; }

    /// <summary>Initial room foreground and background layouts.</summary>
    public RoomVisualLayoutCatalog? RoomVisualLayouts { get; init; }

    /// <summary>Stock native room-level allocations; not overridable.</summary>
    public RoomLevelStreamCatalog? RoomLevelStreams { get; init; }

    /// <summary>Room PLM shot block visuals.</summary>
    public RoomPlmShotBlockVisualCatalog? RoomPlmShotBlockVisuals { get; init; }

    /// <summary>Room PLM grapple block visuals.</summary>
    public RoomPlmGrappleBlockVisualCatalog? RoomPlmGrappleBlockVisuals { get; init; }

    /// <summary>Room PLM station visuals.</summary>
    public RoomPlmStationVisualCatalog? RoomPlmStationVisuals { get; init; }

    /// <summary>Room PLM blue door visuals.</summary>
    public RoomPlmBlueDoorVisualCatalog? RoomPlmBlueDoorVisuals { get; init; }

    /// <summary>Room PLM colored door visuals.</summary>
    public RoomPlmColoredDoorVisualCatalog? RoomPlmColoredDoorVisuals { get; init; }

    /// <summary>Room PLM grey door visuals.</summary>
    public RoomPlmGreyDoorVisualCatalog? RoomPlmGreyDoorVisuals { get; init; }

    /// <summary>Room PLM eye door visuals.</summary>
    public RoomPlmEyeDoorVisualCatalog? RoomPlmEyeDoorVisuals { get; init; }

    /// <summary>Room PLM mother brain glass visuals.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog? RoomPlmMotherBrainGlassVisuals { get; init; }

    /// <summary>Room PLM noob tube visuals.</summary>
    public RoomPlmNoobTubeVisualCatalog? RoomPlmNoobTubeVisuals { get; init; }

    /// <summary>Room PLM downward gate visuals.</summary>
    public RoomPlmDownwardGateVisualCatalog? RoomPlmDownwardGateVisuals { get; init; }

    /// <summary>Room PLM elevator platform visuals.</summary>
    public RoomPlmElevatorPlatformVisualCatalog? RoomPlmElevatorPlatformVisuals { get; init; }

    /// <summary>Room PLM escape gate visuals.</summary>
    public RoomPlmEscapeGateVisualCatalog? RoomPlmEscapeGateVisuals { get; init; }

    /// <summary>Room PLM bomb torizo hand visuals.</summary>
    public RoomPlmBombTorizoHandVisualCatalog? RoomPlmBombTorizoHandVisuals { get; init; }

    /// <summary>Room PLM draygon cannon visuals.</summary>
    public RoomPlmDraygonCannonVisualCatalog? RoomPlmDraygonCannonVisuals { get; init; }

    /// <summary>Room PLM chozo statue visuals.</summary>
    public RoomPlmChozoStatueVisualCatalog? RoomPlmChozoStatueVisuals { get; init; }

    /// <summary>Room PLM linked restore visuals.</summary>
    public RoomPlmLinkedRestoreVisualCatalog? RoomPlmLinkedRestoreVisuals { get; init; }

    /// <summary>Room PLM tourian access visuals.</summary>
    public RoomPlmTourianAccessVisualCatalog? RoomPlmTourianAccessVisuals { get; init; }

    /// <summary>Room PLM speed booster visuals.</summary>
    public RoomPlmSpeedBoosterVisualCatalog? RoomPlmSpeedBoosterVisuals { get; init; }

    /// <summary>Room PLM maridia elevatube visuals.</summary>
    public RoomPlmMaridiaElevatubeVisualCatalog? RoomPlmMaridiaElevatubeVisuals { get; init; }

    /// <summary>Room PLM spore spawn ceiling visuals.</summary>
    public RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals { get; init; }

    /// <summary>Room PLM samus eater visuals.</summary>
    public RoomPlmSamusEaterVisualCatalog? RoomPlmSamusEaterVisuals { get; init; }

    /// <summary>Room PLM botwoon wall visuals.</summary>
    public RoomPlmBotwoonWallVisualCatalog? RoomPlmBotwoonWallVisuals { get; init; }

    /// <summary>Room PLM kraid visuals.</summary>
    public RoomPlmKraidVisualCatalog? RoomPlmKraidVisuals { get; init; }

    /// <summary>Room PLM crocomire visuals.</summary>
    public RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals { get; init; }

    /// <summary>Room PLM mother brain fake death visuals.</summary>
    public RoomPlmMotherBrainFakeDeathVisualCatalog? RoomPlmMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Room PLM collectible visuals.</summary>
    public RoomPlmCollectibleVisualCatalog? RoomPlmCollectibleVisuals { get; init; }

    /// <summary>Room PLM dynamic collectible artwork.</summary>
    public RoomPlmDynamicCollectibleArtCatalog? RoomPlmDynamicCollectibleArt { get; init; }

    /// <summary>X-ray reveal operands and item and special-room overlays.</summary>
    public XrayRevealVisualCatalog? XrayRevealVisuals { get; init; }

    /// <summary>Room background tilemaps.</summary>
    public RoomBackgroundTilemapCatalog? RoomBackgroundTilemapArt { get; init; }

    /// <summary>Room sky tilemaps.</summary>
    public RoomSkyTilemapCatalog? RoomSkyTilemapArt { get; init; }

    /// <summary>Samus projectile sprite compositions.</summary>
    public ProjectileSpriteCatalog? ProjectileCompositions { get; init; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    public ProjectileFrameBindingCatalog? ProjectileFrameBindings { get; init; }

    /// <summary>Beam tiles.</summary>
    public BeamTileCatalog? BeamArtwork { get; init; }

    /// <summary>Enemy sprite sheets, palettes and enemy-projectile spritemaps.</summary>
    public EnemyTileArtworkCatalog? EnemyTileArtwork { get; init; }

    /// <summary>Projectile trail artwork.</summary>
    public ProjectileTrailCatalog? TrailArtwork { get; init; }

    /// <summary>The cartridge bytes Samus's glitch paths read, extracted from the player's cartridge.</summary>
    public SamusOverreadCatalog? SamusOverreads { get; init; }

    /// <summary>Charge-flare placement.</summary>
    public ChargeFlarePlacementCatalog? ChargeFlarePlacement { get; init; }

    /// <summary>Charge-flare sprite compositions.</summary>
    public ChargeFlareSpriteCatalog? ChargeFlareCompositions { get; init; }

    /// <summary>Grapple beam tiles.</summary>
    public GrappleTileAtlas? GrappleArtwork { get; init; }
}
