using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The installed catalogs a gameplay runtime binds (<see cref="SuperMetroidRuntime.BindArtwork"/>). The host's complete
/// catalog set implements it; the runtime never sees the frontend's cinematic and ending catalogs.
/// </summary>
internal interface IGameplayArtwork
{
    /// <summary>Area maps with the pause, HUD, title, intro and ending text and palettes that share their installation.</summary>
    IGameplayPresentationContent? MapPresentation { get; }

    /// <summary>Samus body tiles, palettes and death palettes.</summary>
    InstalledSamusBodyArtwork? SamusBodyArt { get; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    InstalledRoomArtwork? RoomBlockSheets { get; }

    /// <summary>Initial room foreground and background layouts.</summary>
    RoomLevelArtworks? RoomVisualLayouts { get; }

    /// <summary>The installed terrain of every room level; not overridable.</summary>
    InstalledRoomLevelStreams? RoomLevelStreams { get; }

    /// <summary>Room PLM shot block visuals.</summary>
    InstalledRoomObjectShotBlockVisuals? RoomObjectShotBlockVisuals { get; }

    /// <summary>Room PLM grapple block visuals.</summary>
    InstalledRoomObjectGrappleBlockVisuals? RoomObjectGrappleBlockVisuals { get; }

    /// <summary>Room PLM station visuals.</summary>
    InstalledRoomObjectStationVisuals? RoomObjectStationVisuals { get; }

    /// <summary>Room PLM blue door visuals.</summary>
    InstalledRoomObjectBlueDoorVisuals? RoomObjectBlueDoorVisuals { get; }

    /// <summary>Room PLM colored door visuals.</summary>
    InstalledRoomObjectColoredDoorVisuals? RoomObjectColoredDoorVisuals { get; }

    /// <summary>Room PLM grey door visuals.</summary>
    InstalledRoomObjectGreyDoorVisuals? RoomObjectGreyDoorVisuals { get; }

    /// <summary>Room PLM eye door visuals.</summary>
    InstalledRoomObjectEyeDoorVisuals? RoomObjectEyeDoorVisuals { get; }

    /// <summary>Room PLM mother brain glass visuals.</summary>
    RoomObjectMotherBrainGlassVisualCatalog? RoomObjectMotherBrainGlassVisuals { get; }

    /// <summary>Room PLM noob tube visuals.</summary>
    InstalledRoomObjectNoobTubeVisuals? RoomObjectNoobTubeVisuals { get; }

    /// <summary>Room PLM downward gate visuals.</summary>
    InstalledRoomObjectDownwardGateVisuals? RoomObjectDownwardGateVisuals { get; }

    /// <summary>Room PLM elevator platform visuals.</summary>
    RoomObjectElevatorPlatformVisualCatalog? RoomObjectElevatorPlatformVisuals { get; }

    /// <summary>Room PLM escape gate visuals.</summary>
    RoomObjectEscapeGateVisualCatalog? RoomObjectEscapeGateVisuals { get; }

    /// <summary>Room PLM bomb torizo hand visuals.</summary>
    RoomObjectBombTorizoHandVisualCatalog? RoomObjectBombTorizoHandVisuals { get; }

    /// <summary>Room PLM draygon cannon visuals.</summary>
    RoomObjectDraygonCannonVisualCatalog? RoomObjectDraygonCannonVisuals { get; }

    /// <summary>Room PLM chozo statue visuals.</summary>
    RoomObjectChozoStatueVisualCatalog? RoomObjectChozoStatueVisuals { get; }

    /// <summary>Room PLM linked restore visuals.</summary>
    InstalledRoomObjectLinkedRestoreVisuals? RoomObjectLinkedRestoreVisuals { get; }

    /// <summary>Room PLM tourian access visuals.</summary>
    RoomObjectTourianAccessVisuals? RoomObjectTourianAccessVisuals { get; }

    /// <summary>Room PLM speed booster visuals.</summary>
    InstalledRoomObjectSpeedBoosterVisuals? RoomObjectSpeedBoosterVisuals { get; }

    /// <summary>Room PLM maridia elevatube visuals.</summary>
    InstalledRoomObjectMaridiaElevatubeVisuals? RoomObjectMaridiaElevatubeVisuals { get; }

    /// <summary>Room PLM spore spawn ceiling visuals.</summary>
    RoomObjectSporeSpawnCeilingVisualCatalog? RoomObjectSporeSpawnCeilingVisuals { get; }

    /// <summary>Room PLM samus eater visuals.</summary>
    InstalledRoomObjectSamusEaterVisuals? RoomObjectSamusEaterVisuals { get; }

    /// <summary>Room PLM botwoon wall visuals.</summary>
    InstalledRoomObjectBotwoonWallVisuals? RoomObjectBotwoonWallVisuals { get; }

    /// <summary>Room PLM kraid visuals.</summary>
    InstalledRoomObjectKraidVisuals? RoomObjectKraidVisuals { get; }

    /// <summary>Room PLM crocomire visuals.</summary>
    RoomObjectCrocomireVisualCatalog? RoomObjectCrocomireVisuals { get; }

    /// <summary>Room PLM mother brain fake death visuals.</summary>
    RoomObjectMotherBrainFakeDeathVisualCatalog? RoomObjectMotherBrainFakeDeathVisuals { get; }

    /// <summary>Room PLM collectible visuals.</summary>
    InstalledRoomObjectCollectibleVisuals? RoomObjectCollectibleVisuals { get; }

    /// <summary>X-ray reveal operands and item and special-room overlays.</summary>
    InstalledXrayReveals? XrayRevealVisuals { get; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    InstalledProjectileFrameBindings? ProjectileFrameBindings { get; }

    /// <summary>The compatibility services reproducing what Samus's glitch paths read, decoded from the player's game data.</summary>
    SamusOverreadServices? SamusOverreads { get; }

    /// <summary>
    /// The compatibility services reproducing what Kraid's fingernails, Crocomire's volley and the opening cinematic's
    /// egg shell read past their data, decoded from the player's game data.
    /// </summary>
    BossAndIntroOverreadServices? BossAndIntroOverreads { get; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    InstalledSpriteSheets? SpriteSheets { get; }

    /// <summary>The pre-coloured enemy sheets, by the room graphics set that loads each enemy.</summary>
    InstalledEnemySheets? EnemySheets { get; }

    /// <summary>The compatibility services reproducing what enemies read past their authored data, decoded from the player's game data.</summary>
    EnemyOverreadServices? EnemyOverreads { get; }

    /// <summary>Charge-flare placement.</summary>
    ChargeFlarePlacements? ChargeFlarePlacement { get; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    ChargeFlarePlacements? GrappleFlarePlacement { get; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    InstalledGrappleSwingFrames? GrappleSwingFrames { get; }



    /// <summary>Enemy palettes, extended frames and colour catalogs.</summary>
    EnemyTileArtworkCatalog? EnemyTileArtwork { get; }

    /// <summary>The bosses' melts, background frames, corpse characters and colors.</summary>
    BossArtworkCatalog? BossArtwork { get; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    BossBodySheets? BossBodySheets { get; }
}
