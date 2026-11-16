using SuperMetroid.Core.Samus.Abilities.Artwork;
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
/// The installed catalogs a gameplay runtime is built over and binds (#2049): every one is required, since the host installs
/// its artwork before gameplay builds anything that draws (<see cref="Frontend.GameArtwork.Gameplay"/>). The runtime never
/// sees the frontend's cinematic and ending catalogs.
/// </summary>
[ArchitectureReview("context-bag", "The installed artwork set a gameplay runtime is built over, as one value; every member is an immutable artwork catalog, not a service.")]
internal sealed record GameplayArtwork
{
    /// <summary>Area maps with the pause, HUD, title, intro and ending text and colours that share their installation.</summary>
    public required IGameplayPresentationContent MapPresentation { get; init; }

    /// <summary>Samus body pictures, colours and death colours.</summary>
    public required InstalledSamusBodyArtwork SamusBodyArt { get; init; }

    /// <summary>The frames Samus's body can show in each pose and the picture frame each draws.</summary>
    public required SuperMetroid.Core.Samus.Drawing.SamusShownFrames SamusShownFrames { get; init; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public required InstalledRoomArtwork RoomBlockSheets { get; init; }

    /// <summary>Initial room foreground and background layouts.</summary>
    public required RoomLevelArtworks RoomVisualLayouts { get; init; }

    /// <summary>The installed terrain of every room level; not overridable.</summary>
    public required InstalledRoomLevelStreams RoomLevelStreams { get; init; }

    /// <summary>Room-object shot block visuals.</summary>
    public required InstalledRoomObjectShotBlockVisuals RoomObjectShotBlockVisuals { get; init; }

    /// <summary>Room-object grapple block visuals.</summary>
    public required InstalledRoomObjectGrappleBlockVisuals RoomObjectGrappleBlockVisuals { get; init; }

    /// <summary>Room-object station visuals.</summary>
    public required InstalledRoomObjectStationVisuals RoomObjectStationVisuals { get; init; }

    /// <summary>Room-object blue door visuals.</summary>
    public required InstalledRoomObjectBlueDoorVisuals RoomObjectBlueDoorVisuals { get; init; }

    /// <summary>Room-object colored door visuals.</summary>
    public required InstalledRoomObjectColoredDoorVisuals RoomObjectColoredDoorVisuals { get; init; }

    /// <summary>Room-object grey door visuals.</summary>
    public required InstalledRoomObjectGreyDoorVisuals RoomObjectGreyDoorVisuals { get; init; }

    /// <summary>Room-object eye door visuals.</summary>
    public required InstalledRoomObjectEyeDoorVisuals RoomObjectEyeDoorVisuals { get; init; }

    /// <summary>Room-object mother brain glass visuals.</summary>
    public required InstalledMotherBrainGlassFrames RoomObjectMotherBrainGlassVisuals { get; init; }

    /// <summary>Room-object noob tube visuals.</summary>
    public required InstalledRoomObjectNoobTubeVisuals RoomObjectNoobTubeVisuals { get; init; }

    /// <summary>Room-object downward gate visuals.</summary>
    public required InstalledRoomObjectDownwardGateVisuals RoomObjectDownwardGateVisuals { get; init; }

    /// <summary>Room-object elevator platform visuals.</summary>
    public required InstalledRoomObjectElevatorPlatformVisuals RoomObjectElevatorPlatformVisuals { get; init; }

    /// <summary>Room-object escape gate visuals.</summary>
    public required InstalledEscapeGateFrames RoomObjectEscapeGateVisuals { get; init; }

    /// <summary>Room-object bomb torizo hand visuals.</summary>
    public required InstalledRoomObjectBombTorizoHandVisuals RoomObjectBombTorizoHandVisuals { get; init; }

    /// <summary>Room-object draygon cannon visuals.</summary>
    public required InstalledDraygonCannonFrames RoomObjectDraygonCannonVisuals { get; init; }

    /// <summary>Room-object Phantoon door visuals.</summary>
    public required InstalledPhantoonDoorFrames RoomObjectPhantoonDoorVisuals { get; init; }

    /// <summary>Room-object chozo statue visuals.</summary>
    public required InstalledRoomObjectChozoStatueVisuals RoomObjectChozoStatueVisuals { get; init; }

    /// <summary>Room-object linked restore visuals.</summary>
    public required InstalledRoomObjectLinkedRestoreVisuals RoomObjectLinkedRestoreVisuals { get; init; }

    /// <summary>Room-object tourian access visuals.</summary>
    public required RoomObjectTourianAccessVisuals RoomObjectTourianAccessVisuals { get; init; }

    /// <summary>Room-object speed booster visuals.</summary>
    public required InstalledRoomObjectSpeedBoosterVisuals RoomObjectSpeedBoosterVisuals { get; init; }

    /// <summary>Room-object maridia elevatube visuals.</summary>
    public required InstalledRoomObjectMaridiaElevatubeVisuals RoomObjectMaridiaElevatubeVisuals { get; init; }

    /// <summary>Room-object spore spawn ceiling visuals.</summary>
    public required InstalledRoomObjectSporeSpawnCeilingVisuals RoomObjectSporeSpawnCeilingVisuals { get; init; }

    /// <summary>Room-object samus eater visuals.</summary>
    public required InstalledRoomObjectSamusEaterVisuals RoomObjectSamusEaterVisuals { get; init; }

    /// <summary>Room-object botwoon wall visuals.</summary>
    public required InstalledRoomObjectBotwoonWallVisuals RoomObjectBotwoonWallVisuals { get; init; }

    /// <summary>Room-object kraid visuals.</summary>
    public required InstalledRoomObjectKraidVisuals RoomObjectKraidVisuals { get; init; }

    /// <summary>Room-object crocomire visuals.</summary>
    public required InstalledRoomObjectCrocomireVisuals RoomObjectCrocomireVisuals { get; init; }

    /// <summary>Room-object mother brain fake death visuals.</summary>
    public required InstalledMotherBrainFakeDeathFrames RoomObjectMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Room-object collectible visuals.</summary>
    public required InstalledRoomObjectCollectibleVisuals RoomObjectCollectibleVisuals { get; init; }

    /// <summary>Room-object bombed-reveal, escape-passage, escape-shaft-wall and rescue-wall visuals.</summary>
    public required InstalledRoomObjectRevealAndWallVisuals RoomObjectRevealAndWallVisuals { get; init; }

    /// <summary>The blocks room-object setups and block reactions write of their own accord.</summary>
    public required InstalledRoomObjectSetupBlocks RoomObjectSetupBlocks { get; init; }

    /// <summary>X-ray reveal shapes and item and special-room overlays.</summary>
    public required InstalledXrayReveals XrayRevealVisuals { get; init; }

    /// <summary>Timed projectile frame sprite choices.</summary>
    public required InstalledProjectileFrameBindings ProjectileFrameBindings { get; init; }

    /// <summary>The compatibility services reproducing what Samus's glitch paths read, decoded from the player's game data.</summary>
    public required SamusOverreadServices SamusOverreads { get; init; }

    /// <summary>
    /// The compatibility services reproducing what Kraid's fingernails, Crocomire's volley and the opening cinematic's
    /// egg shell read past their data, decoded from the player's game data.
    /// </summary>
    public required BossAndIntroOverreadServices BossAndIntroOverreads { get; init; }

    /// <summary>The pre-coloured sprite sheets drawn families select their frames from.</summary>
    public required InstalledSpriteSheets SpriteSheets { get; init; }

    /// <summary>The pre-coloured enemy artwork: each variant of each enemy, and the variants each population draws (#1599).</summary>
    public required InstalledEnemySheets EnemySheets { get; init; }

    /// <summary>Every installed enemy frame's display, drawn with the enemy sheets.</summary>
    public required EnemyFrameDisplays EnemyFrameDisplays { get; init; }

    /// <summary>The compatibility services reproducing what enemies read past their authored data, decoded from the player's game data.</summary>
    public required EnemyOverreadServices EnemyOverreads { get; init; }

    /// <summary>Charge-flare placement.</summary>
    public required ChargeFlarePlacements ChargeFlarePlacement { get; init; }

    /// <summary>The grapple flare's standing and running offsets.</summary>
    public required ChargeFlarePlacements GrappleFlarePlacement { get; init; }

    /// <summary>The frame Samus's swing pose shows at each grapple angle.</summary>
    public required InstalledGrappleSwingFrames GrappleSwingFrames { get; init; }

    /// <summary>Kraid's eye colours, which his eye glow reads back (#1512, #1583).</summary>
    public required KraidEyeColours KraidEyes { get; init; }

    /// <summary>Every boss's pre-assembled body frames (#1466).</summary>
    public required BossBodySheets BossBodySheets { get; init; }
}
