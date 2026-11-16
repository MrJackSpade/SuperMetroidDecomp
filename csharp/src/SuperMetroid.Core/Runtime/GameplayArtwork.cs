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
    /// <summary>The installed content of the screens, scenes and HUD (#2203): the area maps, the menus, the title, the opening and ending text and the scenes' colours.</summary>
    public required IGameplayPresentationContent ScreenContent { get; init; }

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

    /// <summary>The pictures of the ordinary shot blocks' breakup and restoration frames, which their room objects draw.</summary>
    public required InstalledRoomObjectShotBlockVisuals RoomObjectShotBlockVisuals { get; init; }

    /// <summary>The pictures of the breakable Grapple block's frames, which its room object draws as it breaks and respawns.</summary>
    public required InstalledRoomObjectGrappleBlockVisuals RoomObjectGrappleBlockVisuals { get; init; }

    /// <summary>The pictures of the map, refill and save stations' frames and their access arms' frames.</summary>
    public required InstalledRoomObjectStationVisuals RoomObjectStationVisuals { get; init; }

    /// <summary>The pictures of the blue door caps' frames, closed, opening and closing.</summary>
    public required InstalledRoomObjectBlueDoorVisuals RoomObjectBlueDoorVisuals { get; init; }

    /// <summary>The pictures of the coloured door caps' frames.</summary>
    public required InstalledRoomObjectColoredDoorVisuals RoomObjectColoredDoorVisuals { get; init; }

    /// <summary>The pictures of the grey door caps' frames and their shared clear frames.</summary>
    public required InstalledRoomObjectGreyDoorVisuals RoomObjectGreyDoorVisuals { get; init; }

    /// <summary>The pictures of the eye doors' eye, body, bottom and clearing frames.</summary>
    public required InstalledRoomObjectEyeDoorVisuals RoomObjectEyeDoorVisuals { get; init; }

    /// <summary>The pictures of the frames of Mother Brain's glass as it cracks and shatters.</summary>
    public required InstalledMotherBrainGlassFrames RoomObjectMotherBrainGlassVisuals { get; init; }

    /// <summary>The pictures of the n00b tube's frames as it cracks and breaks.</summary>
    public required InstalledRoomObjectNoobTubeVisuals RoomObjectNoobTubeVisuals { get; init; }

    /// <summary>The pictures of the downward gates' column frames and their coloured triggers.</summary>
    public required InstalledRoomObjectDownwardGateVisuals RoomObjectDownwardGateVisuals { get; init; }

    /// <summary>The pictures of the elevator platform's looping frames.</summary>
    public required InstalledRoomObjectElevatorPlatformVisuals RoomObjectElevatorPlatformVisuals { get; init; }

    /// <summary>The pictures of the open, half-closed and closed frames of the gate in Mother Brain's escape room.</summary>
    public required InstalledEscapeGateFrames RoomObjectEscapeGateVisuals { get; init; }

    /// <summary>The pictures of Bomb Torizo's hand frames, which the hand shows as it opens and closes.</summary>
    public required InstalledRoomObjectBombTorizoHandVisuals RoomObjectBombTorizoHandVisuals { get; init; }

    /// <summary>The pictures of Draygon's wall cannons' frames, whole and destroyed.</summary>
    public required InstalledDraygonCannonFrames RoomObjectDraygonCannonVisuals { get; init; }

    /// <summary>The pictures of Phantoon's door frames: the fight's door and the restored door.</summary>
    public required InstalledPhantoonDoorFrames RoomObjectPhantoonDoorVisuals { get; init; }

    /// <summary>The pictures of the Chozo hand's and its slope access's frames.</summary>
    public required InstalledRoomObjectChozoStatueVisuals RoomObjectChozoStatueVisuals { get; init; }

    /// <summary>The pictures of the bomb and contact-crumble blocks' restoration frames.</summary>
    public required InstalledRoomObjectLinkedRestoreVisuals RoomObjectLinkedRestoreVisuals { get; init; }

    /// <summary>The pictures of the Tourian access floor's frames: the empty row, the crumble rows and the clear.</summary>
    public required RoomObjectTourianAccessVisuals RoomObjectTourianAccessVisuals { get; init; }

    /// <summary>The picture of the Speed Booster block a bomb reveals.</summary>
    public required InstalledRoomObjectSpeedBoosterVisuals RoomObjectSpeedBoosterVisuals { get; init; }

    /// <summary>The picture of the Maridia elevatube's block.</summary>
    public required InstalledRoomObjectMaridiaElevatubeVisuals RoomObjectMaridiaElevatubeVisuals { get; init; }

    /// <summary>The pictures of Spore Spawn's ceiling frames as it crumbles and clears.</summary>
    public required InstalledRoomObjectSporeSpawnCeilingVisuals RoomObjectSporeSpawnCeilingVisuals { get; init; }

    /// <summary>The pictures of the Samus Eater plant's frames.</summary>
    public required InstalledRoomObjectSamusEaterVisuals RoomObjectSamusEaterVisuals { get; init; }

    /// <summary>The picture of the Botwoon wall's clear once it has crumbled.</summary>
    public required InstalledRoomObjectBotwoonWallVisuals RoomObjectBotwoonWallVisuals { get; init; }

    /// <summary>The pictures of Kraid's room's ceiling and floor-spike frames.</summary>
    public required InstalledRoomObjectKraidVisuals RoomObjectKraidVisuals { get; init; }

    /// <summary>The pictures of Crocomire's bridge and invisible-wall frames.</summary>
    public required InstalledRoomObjectCrocomireVisuals RoomObjectCrocomireVisuals { get; init; }

    /// <summary>The pictures of the room changes of Mother Brain's fake death: the wall fill, the escape door, the mural rows, the fill rows and the tube clears.</summary>
    public required InstalledMotherBrainFakeDeathFrames RoomObjectMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>The pictures of the items' and tanks' frames as they lie, are revealed and are collected.</summary>
    public required InstalledRoomObjectCollectibleVisuals RoomObjectCollectibleVisuals { get; init; }

    /// <summary>Room-object bombed-reveal, escape-passage, escape-shaft-wall and rescue-wall visuals.</summary>
    public required InstalledRoomObjectRevealAndWallVisuals RoomObjectRevealAndWallVisuals { get; init; }

    /// <summary>The blocks room-object setups and block reactions write of their own accord.</summary>
    public required InstalledRoomObjectSetupBlocks RoomObjectSetupBlocks { get; init; }

    /// <summary>X-ray reveal shapes and item and special-room overlays.</summary>
    public required InstalledXrayReveals XrayRevealVisuals { get; init; }

    /// <summary>The composition every frame of Samus's shots' animations draws.</summary>
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

    /// <summary>Where the beam's charge flare is drawn from Samus in each pose.</summary>
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
