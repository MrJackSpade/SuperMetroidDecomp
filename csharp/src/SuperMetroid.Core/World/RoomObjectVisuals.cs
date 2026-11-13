using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// The installed pictures of the room's room-object blocks, one catalog per family. A family's draw requires its catalog:
/// drawing with none bound fails, as there is no compiled copy of the pictures to show instead (#1557, #1596).
/// </summary>
[ArchitectureReview("context-bag", "The installed room-object visual catalogs a host binds; every member is immutable artwork data, not a service.")]
internal sealed class RoomObjectVisuals
{
    /// <summary>Visual-only shot-block selection; collision remains compiled.</summary>
    public InstalledRoomObjectShotBlockVisuals? RoomObjectShotBlockVisuals { get; set; }

    /// <summary>Grapple-block art; the compiled terrain owns collision.</summary>
    public InstalledRoomObjectGrappleBlockVisuals? RoomObjectGrappleBlockVisuals { get; set; }

    /// <summary>Station appearance; the compiled terrain retains collision.</summary>
    public InstalledRoomObjectStationVisuals? RoomObjectStationVisuals { get; set; }

    /// <summary>Visual-only blue-door cap frames supplied by installed content.</summary>
    public InstalledRoomObjectBlueDoorVisuals? RoomObjectBlueDoorVisuals { get; set; }

    /// <summary>Visual-only colored-door cap frames supplied by installed content.</summary>
    public InstalledRoomObjectColoredDoorVisuals? RoomObjectColoredDoorVisuals { get; set; }

    /// <summary>Visual-only grey caps and shared clear frames supplied by installed content.</summary>
    public InstalledRoomObjectGreyDoorVisuals? RoomObjectGreyDoorVisuals { get; set; }

    /// <summary>Visual-only mirrored eye-door frames supplied by installed content.</summary>
    public InstalledRoomObjectEyeDoorVisuals? RoomObjectEyeDoorVisuals { get; set; }

    /// <summary>The installed pictures of Mother Brain's glass frames; collision stays with the draws.</summary>
    public InstalledMotherBrainGlassFrames? RoomObjectMotherBrainGlassVisuals { get; set; }

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    public InstalledRoomObjectNoobTubeVisuals? RoomObjectNoobTubeVisuals { get; set; }

    /// <summary>Gate-block appearance; compiled terrain retains collision.</summary>
    public InstalledRoomObjectDownwardGateVisuals? RoomObjectDownwardGateVisuals { get; set; }

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    public InstalledRoomObjectElevatorPlatformVisuals? RoomObjectElevatorPlatformVisuals { get; set; }

    /// <summary>The installed pictures of Mother Brain's escape-gate frames; collision stays with the draws.</summary>
    public InstalledEscapeGateFrames? RoomObjectEscapeGateVisuals { get; set; }

    /// <summary>Bomb Torizo hand art; collision stays with the draws.</summary>
    public InstalledRoomObjectBombTorizoHandVisuals? RoomObjectBombTorizoHandVisuals { get; set; }

    /// <summary>The installed pictures of Draygon's cannon frames; collision stays with the draws.</summary>
    public InstalledDraygonCannonFrames? RoomObjectDraygonCannonVisuals { get; set; }

    /// <summary>The installed pictures of Phantoon's door frames; collision stays with the draws.</summary>
    public InstalledPhantoonDoorFrames? RoomObjectPhantoonDoorVisuals { get; set; }

    /// <summary>Chozo statue room-object art; collision stays with the draws.</summary>
    public InstalledRoomObjectChozoStatueVisuals? RoomObjectChozoStatueVisuals { get; set; }

    /// <summary>Linked-block restoration art; collision stays compiled.</summary>
    public InstalledRoomObjectLinkedRestoreVisuals? RoomObjectLinkedRestoreVisuals { get; set; }

    /// <summary>Tourian access-floor art; the floor's terrain stays compiled.</summary>
    public RoomObjectTourianAccessVisuals? RoomObjectTourianAccessVisuals { get; set; }

    /// <summary>The installed picture of the Speed Booster block reveal; compiled terrain still determines collision and breakability.</summary>
    public InstalledRoomObjectSpeedBoosterVisuals? RoomObjectSpeedBoosterVisuals { get; set; }

    /// <summary>The installed picture of the Maridia elevatube for the draw it shares with Kraid's room, selected by room-object ownership; tube collision and motion remain compiled.</summary>
    public InstalledRoomObjectMaridiaElevatubeVisuals? RoomObjectMaridiaElevatubeVisuals { get; set; }

    /// <summary>The installed pictures of the two-by-two Spore Spawn ceiling clear/crumble draws; their terrain and sequence timing stay compiled.</summary>
    public InstalledRoomObjectSporeSpawnCeilingVisuals? RoomObjectSporeSpawnCeilingVisuals { get; set; }

    /// <summary>The installed pictures of the Samus eater plant; the three-run collision layout stays compiled.</summary>
    public InstalledRoomObjectSamusEaterVisuals? RoomObjectSamusEaterVisuals { get; set; }

    /// <summary>The installed picture of the nine-block vertical Botwoon wall clear; its compiled air-block collision remains unchanged.</summary>
    public InstalledRoomObjectBotwoonWallVisuals? RoomObjectBotwoonWallVisuals { get; set; }

    /// <summary>The installed pictures of Kraid's ceiling and spike frames, distinct from the boss's private body maps and from elevatube owners of the shared draw; block collision and mutation timing remain compiled.</summary>
    public InstalledRoomObjectKraidVisuals? RoomObjectKraidVisuals { get; set; }

    /// <summary>The installed pictures of Crocomire's bridge-clear, crumble and invisible-wall frames; compiled draw runs still own block placement and collision.</summary>
    public InstalledRoomObjectCrocomireVisuals? RoomObjectCrocomireVisuals { get; set; }

    /// <summary>The installed pictures of Mother Brain's fake-death room wall, door, mural rows, fill rows and tube clears; geometry, collision and timing stay with the draws.</summary>
    public InstalledMotherBrainFakeDeathFrames? RoomObjectMotherBrainFakeDeathVisuals { get; set; }

    /// <summary>Collectible art; compiled terrain retains collision.</summary>
    public InstalledRoomObjectCollectibleVisuals? RoomObjectCollectibleVisuals { get; set; }

    /// <summary>The installed pictures of the bombed reveals, the escape passage, the escape shaft wall and the animals' rescue wall.</summary>
    public InstalledRoomObjectRevealAndWallVisuals? RoomObjectRevealAndWallVisuals { get; set; }

    /// <summary>The installed pictures of the blocks room-object setups and block reactions write of their own accord (#1580).</summary>
    public InstalledRoomObjectSetupBlocks? RoomObjectSetupBlocks { get; set; }
}
