using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
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
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
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

    /// <summary>Samus body pictures, palettes and death palettes.</summary>
    InstalledSamusBodyArtwork? SamusBodyArt { get; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    InstalledRoomArtwork? RoomBlockSheets { get; }

    /// <summary>Initial room foreground and background layouts.</summary>
    RoomLevelArtworks? RoomVisualLayouts { get; }

    /// <summary>The installed terrain of every room level; not overridable.</summary>
    InstalledRoomLevelStreams? RoomLevelStreams { get; }

    /// <summary>Room-object shot block visuals.</summary>
    InstalledRoomObjectShotBlockVisuals? RoomObjectShotBlockVisuals { get; }

    /// <summary>Room-object grapple block visuals.</summary>
    InstalledRoomObjectGrappleBlockVisuals? RoomObjectGrappleBlockVisuals { get; }

    /// <summary>Room-object station visuals.</summary>
    InstalledRoomObjectStationVisuals? RoomObjectStationVisuals { get; }

    /// <summary>Room-object blue door visuals.</summary>
    InstalledRoomObjectBlueDoorVisuals? RoomObjectBlueDoorVisuals { get; }

    /// <summary>Room-object colored door visuals.</summary>
    InstalledRoomObjectColoredDoorVisuals? RoomObjectColoredDoorVisuals { get; }

    /// <summary>Room-object grey door visuals.</summary>
    InstalledRoomObjectGreyDoorVisuals? RoomObjectGreyDoorVisuals { get; }

    /// <summary>Room-object eye door visuals.</summary>
    InstalledRoomObjectEyeDoorVisuals? RoomObjectEyeDoorVisuals { get; }

    /// <summary>Room-object mother brain glass visuals.</summary>
    InstalledMotherBrainGlassFrames? RoomObjectMotherBrainGlassVisuals { get; }

    /// <summary>Room-object noob tube visuals.</summary>
    InstalledRoomObjectNoobTubeVisuals? RoomObjectNoobTubeVisuals { get; }

    /// <summary>Room-object downward gate visuals.</summary>
    InstalledRoomObjectDownwardGateVisuals? RoomObjectDownwardGateVisuals { get; }

    /// <summary>Room-object elevator platform visuals.</summary>
    InstalledRoomObjectElevatorPlatformVisuals? RoomObjectElevatorPlatformVisuals { get; }

    /// <summary>Room-object escape gate visuals.</summary>
    InstalledEscapeGateFrames? RoomObjectEscapeGateVisuals { get; }

    /// <summary>Room-object bomb torizo hand visuals.</summary>
    InstalledRoomObjectBombTorizoHandVisuals? RoomObjectBombTorizoHandVisuals { get; }

    /// <summary>Room-object draygon cannon visuals.</summary>
    InstalledDraygonCannonFrames? RoomObjectDraygonCannonVisuals { get; }

    /// <summary>Room-object Phantoon door visuals.</summary>
    InstalledPhantoonDoorFrames? RoomObjectPhantoonDoorVisuals { get; }

    /// <summary>Room-object chozo statue visuals.</summary>
    InstalledRoomObjectChozoStatueVisuals? RoomObjectChozoStatueVisuals { get; }

    /// <summary>Room-object linked restore visuals.</summary>
    InstalledRoomObjectLinkedRestoreVisuals? RoomObjectLinkedRestoreVisuals { get; }

    /// <summary>Room-object tourian access visuals.</summary>
    RoomObjectTourianAccessVisuals? RoomObjectTourianAccessVisuals { get; }

    /// <summary>Room-object speed booster visuals.</summary>
    InstalledRoomObjectSpeedBoosterVisuals? RoomObjectSpeedBoosterVisuals { get; }

    /// <summary>Room-object maridia elevatube visuals.</summary>
    InstalledRoomObjectMaridiaElevatubeVisuals? RoomObjectMaridiaElevatubeVisuals { get; }

    /// <summary>Room-object spore spawn ceiling visuals.</summary>
    InstalledRoomObjectSporeSpawnCeilingVisuals? RoomObjectSporeSpawnCeilingVisuals { get; }

    /// <summary>Room-object samus eater visuals.</summary>
    InstalledRoomObjectSamusEaterVisuals? RoomObjectSamusEaterVisuals { get; }

    /// <summary>Room-object botwoon wall visuals.</summary>
    InstalledRoomObjectBotwoonWallVisuals? RoomObjectBotwoonWallVisuals { get; }

    /// <summary>Room-object kraid visuals.</summary>
    InstalledRoomObjectKraidVisuals? RoomObjectKraidVisuals { get; }

    /// <summary>Room-object crocomire visuals.</summary>
    InstalledRoomObjectCrocomireVisuals? RoomObjectCrocomireVisuals { get; }

    /// <summary>Room-object mother brain fake death visuals.</summary>
    InstalledMotherBrainFakeDeathFrames? RoomObjectMotherBrainFakeDeathVisuals { get; }

    /// <summary>Room-object collectible visuals.</summary>
    InstalledRoomObjectCollectibleVisuals? RoomObjectCollectibleVisuals { get; }

    /// <summary>Room-object bombed-reveal, escape-passage, escape-shaft-wall and rescue-wall visuals.</summary>
    InstalledRoomObjectRevealAndWallVisuals? RoomObjectRevealAndWallVisuals { get; }

    /// <summary>The blocks room-object setups and block reactions write of their own accord.</summary>
    InstalledRoomObjectSetupBlocks? RoomObjectSetupBlocks { get; }

    /// <summary>X-ray reveal shapes and item and special-room overlays.</summary>
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

    /// <summary>The pre-coloured enemy artwork: each variant of each enemy, and the variants each population draws (#1599).</summary>
    InstalledEnemySheets? EnemySheets { get; }

    /// <summary>Every installed enemy frame's display, drawn with the enemy sheets.</summary>
    EnemyFrameDisplays? EnemyFrameDisplays { get; }

    /// <summary>The compatibility services reproducing what enemies read past their authored data, decoded from the player's game data.</summary>
    EnemyOverreadServices? EnemyOverreads { get; }

    /// <summary>Charge-flare placement.</summary>
    ChargeFlarePlacements? ChargeFlarePlacement { get; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    ChargeFlarePlacements? GrappleFlarePlacement { get; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    InstalledGrappleSwingFrames? GrappleSwingFrames { get; }



    /// <summary>Kraid's eye colours, which his eye glow reads back (#1512, #1583).</summary>
    KraidEyeColours? KraidEyes { get; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    BossBodySheets? BossBodySheets { get; }
}
