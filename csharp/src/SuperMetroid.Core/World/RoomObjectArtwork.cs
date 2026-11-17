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
/// The installed pictures of every room-object family's blocks (#2034, #2290), one catalog per family, each required: the
/// installation holds every family's pictures, so there is no family a room draws without them.
/// </summary>
[ArchitectureReview("context-bag", "The installed room-object pictures as one value (#2290), as GameplayArtwork carries its catalogs; every member is an immutable picture catalog of one family, not a service.")]
internal sealed record RoomObjectArtwork
{
    /// <summary>Visual-only shot-block selection; collision remains compiled.</summary>
    public required InstalledRoomObjectShotBlockVisuals RoomObjectShotBlockVisuals { get; init; }

    /// <summary>Grapple-block art; the compiled terrain owns collision.</summary>
    public required InstalledRoomObjectGrappleBlockVisuals RoomObjectGrappleBlockVisuals { get; init; }

    /// <summary>Station appearance; the compiled terrain retains collision.</summary>
    public required InstalledRoomObjectStationVisuals RoomObjectStationVisuals { get; init; }

    /// <summary>Visual-only blue-door cap frames supplied by installed content.</summary>
    public required InstalledRoomObjectBlueDoorVisuals RoomObjectBlueDoorVisuals { get; init; }

    /// <summary>Visual-only colored-door cap frames supplied by installed content.</summary>
    public required InstalledRoomObjectColoredDoorVisuals RoomObjectColoredDoorVisuals { get; init; }

    /// <summary>Visual-only grey caps and shared clear frames supplied by installed content.</summary>
    public required InstalledRoomObjectGreyDoorVisuals RoomObjectGreyDoorVisuals { get; init; }

    /// <summary>Visual-only mirrored eye-door frames supplied by installed content.</summary>
    public required InstalledRoomObjectEyeDoorVisuals RoomObjectEyeDoorVisuals { get; init; }

    /// <summary>The installed pictures of Mother Brain's glass frames; collision stays with the draws.</summary>
    public required InstalledMotherBrainGlassFrames RoomObjectMotherBrainGlassVisuals { get; init; }

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    public required InstalledRoomObjectNoobTubeVisuals RoomObjectNoobTubeVisuals { get; init; }

    /// <summary>Gate-block appearance; compiled terrain retains collision.</summary>
    public required InstalledRoomObjectDownwardGateVisuals RoomObjectDownwardGateVisuals { get; init; }

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    public required InstalledRoomObjectElevatorPlatformVisuals RoomObjectElevatorPlatformVisuals { get; init; }

    /// <summary>The installed pictures of Mother Brain's escape-gate frames; collision stays with the draws.</summary>
    public required InstalledEscapeGateFrames RoomObjectEscapeGateVisuals { get; init; }

    /// <summary>Bomb Torizo hand art; collision stays with the draws.</summary>
    public required InstalledRoomObjectBombTorizoHandVisuals RoomObjectBombTorizoHandVisuals { get; init; }

    /// <summary>The installed pictures of Draygon's cannon frames; collision stays with the draws.</summary>
    public required InstalledDraygonCannonFrames RoomObjectDraygonCannonVisuals { get; init; }

    /// <summary>The installed pictures of Phantoon's door frames; collision stays with the draws.</summary>
    public required InstalledPhantoonDoorFrames RoomObjectPhantoonDoorVisuals { get; init; }

    /// <summary>Chozo statue room-object art; collision stays with the draws.</summary>
    public required InstalledRoomObjectChozoStatueVisuals RoomObjectChozoStatueVisuals { get; init; }

    /// <summary>Linked-block restoration art; collision stays compiled.</summary>
    public required InstalledRoomObjectLinkedRestoreVisuals RoomObjectLinkedRestoreVisuals { get; init; }

    /// <summary>Tourian access-floor art; the floor's terrain stays compiled.</summary>
    public required RoomObjectTourianAccessVisuals RoomObjectTourianAccessVisuals { get; init; }

    /// <summary>The installed picture of the Speed Booster block reveal; compiled terrain still determines collision and breakability.</summary>
    public required InstalledRoomObjectSpeedBoosterVisuals RoomObjectSpeedBoosterVisuals { get; init; }

    /// <summary>The installed picture of the Maridia elevatube for the draw it shares with Kraid's room, selected by room-object ownership; tube collision and motion remain compiled.</summary>
    public required InstalledRoomObjectMaridiaElevatubeVisuals RoomObjectMaridiaElevatubeVisuals { get; init; }

    /// <summary>The installed pictures of the two-by-two Spore Spawn ceiling clear/crumble draws; their terrain and sequence timing stay compiled.</summary>
    public required InstalledRoomObjectSporeSpawnCeilingVisuals RoomObjectSporeSpawnCeilingVisuals { get; init; }

    /// <summary>The installed pictures of the Samus eater plant; the three-run collision layout stays compiled.</summary>
    public required InstalledRoomObjectSamusEaterVisuals RoomObjectSamusEaterVisuals { get; init; }

    /// <summary>The installed picture of the nine-block vertical Botwoon wall clear; its compiled air-block collision remains unchanged.</summary>
    public required InstalledRoomObjectBotwoonWallVisuals RoomObjectBotwoonWallVisuals { get; init; }

    /// <summary>The installed pictures of Kraid's ceiling and spike frames, distinct from the boss's own body frames and from elevatube owners of the shared draw; block collision and mutation timing remain compiled.</summary>
    public required InstalledRoomObjectKraidVisuals RoomObjectKraidVisuals { get; init; }

    /// <summary>The installed pictures of Crocomire's bridge-clear, crumble and invisible-wall frames; compiled draw runs still own block placement and collision.</summary>
    public required InstalledRoomObjectCrocomireVisuals RoomObjectCrocomireVisuals { get; init; }

    /// <summary>The installed pictures of Mother Brain's fake-death room wall, door, mural rows, fill rows and tube clears; geometry, collision and timing stay with the draws.</summary>
    public required InstalledMotherBrainFakeDeathFrames RoomObjectMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Collectible art; compiled terrain retains collision.</summary>
    public required InstalledRoomObjectCollectibleVisuals RoomObjectCollectibleVisuals { get; init; }

    /// <summary>The installed pictures of the bombed reveals, the escape passage, the escape shaft wall and the animals' rescue wall.</summary>
    public required InstalledRoomObjectRevealAndWallVisuals RoomObjectRevealAndWallVisuals { get; init; }

    /// <summary>The installed pictures of the blocks room-object setups and block reactions write of their own accord (#1580).</summary>
    public required InstalledRoomObjectSetupBlocks RoomObjectSetupBlocks { get; init; }

    /// <summary>Every catalog, each under its own type, which names the one family it pictures.</summary>
    internal IReadOnlyList<object> Catalogs =>
    [
        RoomObjectShotBlockVisuals,
        RoomObjectGrappleBlockVisuals,
        RoomObjectStationVisuals,
        RoomObjectBlueDoorVisuals,
        RoomObjectColoredDoorVisuals,
        RoomObjectGreyDoorVisuals,
        RoomObjectEyeDoorVisuals,
        RoomObjectMotherBrainGlassVisuals,
        RoomObjectNoobTubeVisuals,
        RoomObjectDownwardGateVisuals,
        RoomObjectElevatorPlatformVisuals,
        RoomObjectEscapeGateVisuals,
        RoomObjectBombTorizoHandVisuals,
        RoomObjectDraygonCannonVisuals,
        RoomObjectPhantoonDoorVisuals,
        RoomObjectChozoStatueVisuals,
        RoomObjectLinkedRestoreVisuals,
        RoomObjectTourianAccessVisuals,
        RoomObjectSpeedBoosterVisuals,
        RoomObjectMaridiaElevatubeVisuals,
        RoomObjectSporeSpawnCeilingVisuals,
        RoomObjectSamusEaterVisuals,
        RoomObjectBotwoonWallVisuals,
        RoomObjectKraidVisuals,
        RoomObjectCrocomireVisuals,
        RoomObjectMotherBrainFakeDeathVisuals,
        RoomObjectCollectibleVisuals,
        RoomObjectRevealAndWallVisuals,
        RoomObjectSetupBlocks,
    ];
}
