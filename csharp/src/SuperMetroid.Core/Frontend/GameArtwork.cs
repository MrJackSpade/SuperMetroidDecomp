using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
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
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Every installed presentation catalog the frontend binds: maps, room and PLM art, cinematic, ending, Samus,
/// enemy and projectile artwork. Hosts load one set from an installation and bind it with
/// <see cref="SuperMetroidGame.BindArtwork"/>; a restored debugger state rebinds the same set.
/// </summary>
[ArchitectureReview("context-bag", "The installed artwork set a host binds as one value; every member is an immutable artwork catalog, not a service.")]
public sealed record GameArtwork : IGameplayArtwork
{
    /// <summary>No catalog bound: the state of a new game, or of a restored one before its host rebinds.</summary>
    public static GameArtwork Unbound { get; } = new();

    /// <summary>Area maps with the pause, HUD, title, intro and ending text and palettes that share their installation.</summary>
    public AreaMapPresentation? MapPresentation { get; init; }

    /// <inheritdoc/>
    IGameplayPresentationContent? IGameplayArtwork.MapPresentation => MapPresentation;

    /// <summary>Gameplay base palettes a new runtime starts from.</summary>
    public GameplayBasePalette? GameplayBasePalettes { get; init; }

    /// <summary>Common gameplay sprite characters (standard objects).</summary>
    public RoomCharacterAtlas? StandardObjectArt { get; init; }

    /// <summary>Intro, Ceres destruction and Ceres flight artwork.</summary>
    public IntroCinematicArtwork? IntroCinematicArt { get; init; }

    /// <summary>Samus body tiles, palettes and death palettes.</summary>
    public InstalledSamusBodyArtwork? SamusBodyArt { get; init; }

    /// <summary>Ending Mode 7 artwork.</summary>
    public EndingFloorArtwork? EndingFloorArt { get; init; }

    /// <summary>Ending sprite artwork.</summary>
    public EndingObjectArtwork? EndingObjectArt { get; init; }

    /// <summary>Ending palettes.</summary>
    public EndingPaletteSet? EndingPaletteArt { get; init; }

    /// <summary>Room tileset characters.</summary>
    public RoomCharacterAtlases? RoomCharacterArt { get; init; }

    /// <summary>Room static palettes.</summary>
    public RoomStaticPalettes? RoomPaletteArt { get; init; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork? RoomBlockSheets { get; init; }

    /// <summary>Initial room foreground and background layouts.</summary>
    public RoomLevelArtworks? RoomVisualLayouts { get; init; }

    /// <summary>Stock native room-level allocations; not overridable.</summary>
    public InstalledRoomLevelStreams? RoomLevelStreams { get; init; }

    /// <summary>Room PLM shot block visuals.</summary>
    public InstalledRoomPlmShotBlockVisuals? RoomPlmShotBlockVisuals { get; init; }

    /// <summary>Room PLM grapple block visuals.</summary>
    public InstalledRoomPlmGrappleBlockVisuals? RoomPlmGrappleBlockVisuals { get; init; }

    /// <summary>Room PLM station visuals.</summary>
    public InstalledRoomPlmStationVisuals? RoomPlmStationVisuals { get; init; }

    /// <summary>Room PLM blue door visuals.</summary>
    public InstalledRoomPlmBlueDoorVisuals? RoomPlmBlueDoorVisuals { get; init; }

    /// <summary>Room PLM colored door visuals.</summary>
    public InstalledRoomPlmColoredDoorVisuals? RoomPlmColoredDoorVisuals { get; init; }

    /// <summary>Room PLM grey door visuals.</summary>
    public InstalledRoomPlmGreyDoorVisuals? RoomPlmGreyDoorVisuals { get; init; }

    /// <summary>Room PLM eye door visuals.</summary>
    public InstalledRoomPlmEyeDoorVisuals? RoomPlmEyeDoorVisuals { get; init; }

    /// <summary>Room PLM mother brain glass visuals.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog? RoomPlmMotherBrainGlassVisuals { get; init; }

    /// <summary>Room PLM noob tube visuals.</summary>
    public InstalledRoomPlmNoobTubeVisuals? RoomPlmNoobTubeVisuals { get; init; }

    /// <summary>Room PLM downward gate visuals.</summary>
    public InstalledRoomPlmDownwardGateVisuals? RoomPlmDownwardGateVisuals { get; init; }

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
    public InstalledRoomPlmLinkedRestoreVisuals? RoomPlmLinkedRestoreVisuals { get; init; }

    /// <summary>Room PLM tourian access visuals.</summary>
    public RoomPlmTourianAccessVisuals? RoomPlmTourianAccessVisuals { get; init; }

    /// <summary>Room PLM speed booster visuals.</summary>
    public InstalledRoomPlmSpeedBoosterVisuals? RoomPlmSpeedBoosterVisuals { get; init; }

    /// <summary>Room PLM maridia elevatube visuals.</summary>
    public InstalledRoomPlmMaridiaElevatubeVisuals? RoomPlmMaridiaElevatubeVisuals { get; init; }

    /// <summary>Room PLM spore spawn ceiling visuals.</summary>
    public RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals { get; init; }

    /// <summary>Room PLM samus eater visuals.</summary>
    public InstalledRoomPlmSamusEaterVisuals? RoomPlmSamusEaterVisuals { get; init; }

    /// <summary>Room PLM botwoon wall visuals.</summary>
    public InstalledRoomPlmBotwoonWallVisuals? RoomPlmBotwoonWallVisuals { get; init; }

    /// <summary>Room PLM kraid visuals.</summary>
    public InstalledRoomPlmKraidVisuals? RoomPlmKraidVisuals { get; init; }

    /// <summary>Room PLM crocomire visuals.</summary>
    public RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals { get; init; }

    /// <summary>Room PLM mother brain fake death visuals.</summary>
    public RoomPlmMotherBrainFakeDeathVisualCatalog? RoomPlmMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Room PLM collectible visuals.</summary>
    public InstalledRoomPlmCollectibleVisuals? RoomPlmCollectibleVisuals { get; init; }

    /// <summary>X-ray reveal operands and item and special-room overlays.</summary>
    public InstalledXrayReveals? XrayRevealVisuals { get; init; }

    /// <summary>Room background tilemaps.</summary>
    public RoomBackgroundTilemaps? RoomBackgroundTilemapArt { get; init; }

    /// <summary>Room sky tilemaps.</summary>
    public RoomSkyTilemaps? RoomSkyTilemapArt { get; init; }

    /// <summary>Samus projectile sprite compositions.</summary>
    public InstalledProjectileSprites? ProjectileCompositions { get; init; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    public InstalledProjectileFrameBindings? ProjectileFrameBindings { get; init; }

    /// <summary>The beam palettes, by beam combination.</summary>
    public InstalledBeamPalettes? BeamPalettes { get; init; }

    /// <summary>The Hyper Beam palette effect's colour cycle.</summary>
    public InstalledHyperBeamFxColors? HyperBeamFxColors { get; init; }

    /// <summary>Enemy sprite sheets, palettes and enemy-projectile spritemaps.</summary>
    public EnemyTileArtworkCatalog? EnemyTileArtwork { get; init; }

    /// <summary>The bosses' melts, skeletons, BG2 frames, special pages and colors.</summary>
    public BossArtworkCatalog? BossArtwork { get; init; }

    /// <summary>The Ceres door and escape artwork and the Tourian statues' colors.</summary>
    public AreaArtworkBundle? AreaArtwork { get; init; }

    /// <summary>Projectile trail artwork.</summary>
    public InstalledProjectileTrails? TrailArtwork { get; init; }

    /// <summary>The cartridge bytes Samus's glitch paths read, extracted from the player's cartridge.</summary>
    public InstalledSamusOverreads? SamusOverreads { get; init; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    public InstalledSpriteSheets? SpriteSheets { get; init; }

    /// <summary>The pre-coloured enemy sheets, by the room graphics set that loads each enemy.</summary>
    public InstalledEnemySheets? EnemySheets { get; init; }

    /// <summary>Charge-flare placement.</summary>
    public ChargeFlarePlacements? ChargeFlarePlacement { get; init; }

    /// <summary>Charge-flare sprite compositions.</summary>
    public ChargeFlareSprites? ChargeFlareCompositions { get; init; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    public ChargeFlarePlacements? GrappleFlarePlacement { get; init; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    public InstalledGrappleSwingFrames? GrappleSwingFrames { get; init; }
}
