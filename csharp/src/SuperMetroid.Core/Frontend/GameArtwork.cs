using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
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
/// <see cref="SuperMetroidGame.BindArtwork"/>.
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

    /// <summary>Intro, Ceres destruction and Ceres flight artwork.</summary>
    public IntroCinematicArtwork? IntroCinematicArt { get; init; }

    /// <summary>Samus body tiles, palettes and death palettes.</summary>
    public InstalledSamusBodyArtwork? SamusBodyArt { get; init; }

    /// <summary>Ending sprite artwork.</summary>
    public EndingObjectArtwork? EndingObjectArt { get; init; }

    /// <summary>The ending scenes' backdrops (#1544).</summary>
    public EndingBackdrops? EndingBackdrops { get; init; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork? RoomBlockSheets { get; init; }

    /// <summary>Initial room foreground and background layouts.</summary>
    public RoomLevelArtworks? RoomVisualLayouts { get; init; }

    /// <summary>The installed terrain of every room level; not overridable.</summary>
    public InstalledRoomLevelStreams? RoomLevelStreams { get; init; }

    /// <summary>Room PLM shot block visuals.</summary>
    public InstalledRoomObjectShotBlockVisuals? RoomObjectShotBlockVisuals { get; init; }

    /// <summary>Room PLM grapple block visuals.</summary>
    public InstalledRoomObjectGrappleBlockVisuals? RoomObjectGrappleBlockVisuals { get; init; }

    /// <summary>Room PLM station visuals.</summary>
    public InstalledRoomObjectStationVisuals? RoomObjectStationVisuals { get; init; }

    /// <summary>Room PLM blue door visuals.</summary>
    public InstalledRoomObjectBlueDoorVisuals? RoomObjectBlueDoorVisuals { get; init; }

    /// <summary>Room PLM colored door visuals.</summary>
    public InstalledRoomObjectColoredDoorVisuals? RoomObjectColoredDoorVisuals { get; init; }

    /// <summary>Room PLM grey door visuals.</summary>
    public InstalledRoomObjectGreyDoorVisuals? RoomObjectGreyDoorVisuals { get; init; }

    /// <summary>Room PLM eye door visuals.</summary>
    public InstalledRoomObjectEyeDoorVisuals? RoomObjectEyeDoorVisuals { get; init; }

    /// <summary>Room PLM mother brain glass visuals.</summary>
    public RoomObjectMotherBrainGlassVisualCatalog? RoomObjectMotherBrainGlassVisuals { get; init; }

    /// <summary>Room PLM noob tube visuals.</summary>
    public InstalledRoomObjectNoobTubeVisuals? RoomObjectNoobTubeVisuals { get; init; }

    /// <summary>Room PLM downward gate visuals.</summary>
    public InstalledRoomObjectDownwardGateVisuals? RoomObjectDownwardGateVisuals { get; init; }

    /// <summary>Room PLM elevator platform visuals.</summary>
    public RoomObjectElevatorPlatformVisualCatalog? RoomObjectElevatorPlatformVisuals { get; init; }

    /// <summary>Room PLM escape gate visuals.</summary>
    public RoomObjectEscapeGateVisualCatalog? RoomObjectEscapeGateVisuals { get; init; }

    /// <summary>Room PLM bomb torizo hand visuals.</summary>
    public RoomObjectBombTorizoHandVisualCatalog? RoomObjectBombTorizoHandVisuals { get; init; }

    /// <summary>Room PLM draygon cannon visuals.</summary>
    public RoomObjectDraygonCannonVisualCatalog? RoomObjectDraygonCannonVisuals { get; init; }

    /// <summary>Room PLM chozo statue visuals.</summary>
    public RoomObjectChozoStatueVisualCatalog? RoomObjectChozoStatueVisuals { get; init; }

    /// <summary>Room PLM linked restore visuals.</summary>
    public InstalledRoomObjectLinkedRestoreVisuals? RoomObjectLinkedRestoreVisuals { get; init; }

    /// <summary>Room PLM tourian access visuals.</summary>
    public RoomObjectTourianAccessVisuals? RoomObjectTourianAccessVisuals { get; init; }

    /// <summary>Room PLM speed booster visuals.</summary>
    public InstalledRoomObjectSpeedBoosterVisuals? RoomObjectSpeedBoosterVisuals { get; init; }

    /// <summary>Room PLM maridia elevatube visuals.</summary>
    public InstalledRoomObjectMaridiaElevatubeVisuals? RoomObjectMaridiaElevatubeVisuals { get; init; }

    /// <summary>Room PLM spore spawn ceiling visuals.</summary>
    public RoomObjectSporeSpawnCeilingVisualCatalog? RoomObjectSporeSpawnCeilingVisuals { get; init; }

    /// <summary>Room PLM samus eater visuals.</summary>
    public InstalledRoomObjectSamusEaterVisuals? RoomObjectSamusEaterVisuals { get; init; }

    /// <summary>Room PLM botwoon wall visuals.</summary>
    public InstalledRoomObjectBotwoonWallVisuals? RoomObjectBotwoonWallVisuals { get; init; }

    /// <summary>Room PLM kraid visuals.</summary>
    public InstalledRoomObjectKraidVisuals? RoomObjectKraidVisuals { get; init; }

    /// <summary>Room PLM crocomire visuals.</summary>
    public RoomObjectCrocomireVisualCatalog? RoomObjectCrocomireVisuals { get; init; }

    /// <summary>Room PLM mother brain fake death visuals.</summary>
    public RoomObjectMotherBrainFakeDeathVisualCatalog? RoomObjectMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Room PLM collectible visuals.</summary>
    public InstalledRoomObjectCollectibleVisuals? RoomObjectCollectibleVisuals { get; init; }

    /// <summary>Room PLM bombed-reveal, escape-passage, escape-shaft-wall and rescue-wall visuals.</summary>
    public InstalledRoomObjectRevealAndWallVisuals? RoomObjectRevealAndWallVisuals { get; init; }

    /// <summary>X-ray reveal operands and item and special-room overlays.</summary>
    public InstalledXrayReveals? XrayRevealVisuals { get; init; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    public InstalledProjectileFrameBindings? ProjectileFrameBindings { get; init; }


    /// <summary>Enemy palettes, extended frames and colour catalogs.</summary>
    public EnemyTileArtworkCatalog? EnemyTileArtwork { get; init; }

    /// <summary>The bosses' melts, background body frames, corpse characters and colors.</summary>
    public BossArtworkCatalog? BossArtwork { get; init; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    public BossBodySheets? BossBodySheets { get; init; }

    /// <summary>The compatibility services reproducing what Samus's glitch paths read, decoded from the player's game data.</summary>
    public SamusOverreadServices? SamusOverreads { get; init; }

    /// <summary>
    /// The compatibility services reproducing what Kraid's fingernails, Crocomire's volley and the opening cinematic's
    /// egg shell read past their data, decoded from the player's game data.
    /// </summary>
    public BossAndIntroOverreadServices? BossAndIntroOverreads { get; init; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    public InstalledSpriteSheets? SpriteSheets { get; init; }

    /// <summary>The pre-coloured enemy sheets, by the room graphics set that loads each enemy.</summary>
    public InstalledEnemySheets? EnemySheets { get; init; }

    /// <summary>The compatibility services reproducing what enemies read past their authored data, decoded from the player's game data.</summary>
    public EnemyOverreadServices? EnemyOverreads { get; init; }

    /// <summary>Charge-flare placement.</summary>
    public ChargeFlarePlacements? ChargeFlarePlacement { get; init; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    public ChargeFlarePlacements? GrappleFlarePlacement { get; init; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    public InstalledGrappleSwingFrames? GrappleSwingFrames { get; init; }
}
