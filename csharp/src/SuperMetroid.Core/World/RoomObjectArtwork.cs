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
    /// <summary>Visual-only shot-block selection; collision is the terrain's.</summary>
    public required InstalledRoomObjectPictures RoomObjectShotBlockVisuals { get; init; }

    /// <summary>Grapple-block art; the terrain owns collision.</summary>
    public required InstalledRoomObjectPictures RoomObjectGrappleBlockVisuals { private get; init; }

    /// <summary>Station appearance; the terrain retains collision.</summary>
    public required InstalledRoomObjectPictures RoomObjectStationVisuals { get; init; }

    /// <summary>Visual-only blue-door cap frames supplied by installed content.</summary>
    public required InstalledRoomObjectBlueDoorVisuals RoomObjectBlueDoorVisuals { get; init; }

    /// <summary>Visual-only colored-door cap frames supplied by installed content.</summary>
    public required InstalledRoomObjectPictures RoomObjectColoredDoorVisuals { get; init; }

    /// <summary>Visual-only grey caps and shared clear frames supplied by installed content.</summary>
    public required InstalledRoomObjectPictures RoomObjectGreyDoorVisuals { get; init; }

    /// <summary>Visual-only mirrored eye-door frames supplied by installed content.</summary>
    public required InstalledRoomObjectEyeDoorVisuals RoomObjectEyeDoorVisuals { get; init; }

    /// <summary>The installed pictures of Mother Brain's glass frames; collision stays with the draws.</summary>
    public required InstalledMotherBrainGlassFrames RoomObjectMotherBrainGlassVisuals { get; init; }

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    public required InstalledRoomObjectPictures RoomObjectNoobTubeVisuals { get; init; }

    /// <summary>Gate-block appearance; the terrain retains collision.</summary>
    public required InstalledRoomObjectPictures RoomObjectDownwardGateVisuals { get; init; }

    /// <summary>Visual-only elevator frames; platform collision and timing are the elevator's own.</summary>
    public required InstalledRoomObjectPictures RoomObjectElevatorPlatformVisuals { get; init; }

    /// <summary>The installed pictures of Mother Brain's escape-gate frames; collision stays with the draws.</summary>
    public required InstalledEscapeGateFrames RoomObjectEscapeGateVisuals { get; init; }

    /// <summary>Bomb Torizo hand art; collision stays with the draws.</summary>
    public required InstalledRoomObjectPictures RoomObjectBombTorizoHandVisuals { get; init; }

    /// <summary>The installed pictures of Draygon's cannon frames; collision stays with the draws.</summary>
    public required InstalledDraygonCannonFrames RoomObjectDraygonCannonVisuals { get; init; }

    /// <summary>The installed pictures of Phantoon's door frames; collision stays with the draws.</summary>
    public required InstalledPhantoonDoorFrames RoomObjectPhantoonDoorVisuals { get; init; }

    /// <summary>Chozo statue room-object art; collision stays with the draws.</summary>
    public required InstalledRoomObjectPictures RoomObjectChozoStatueVisuals { get; init; }

    /// <summary>Linked-block restoration art; collision stays the terrain's.</summary>
    public required InstalledRoomObjectPictures RoomObjectLinkedRestoreVisuals { get; init; }

    /// <summary>Tourian access-floor art; the floor's terrain owns its collision.</summary>
    public required RoomObjectTourianAccessVisuals RoomObjectTourianAccessVisuals { get; init; }

    /// <summary>The installed picture of the Speed Booster block reveal; the terrain still determines collision and breakability.</summary>
    public required InstalledRoomObjectPictures RoomObjectSpeedBoosterVisuals { get; init; }

    /// <summary>The installed picture of the Maridia elevatube for the draw it shares with Kraid's room, selected by room-object ownership; tube collision and motion are the tube's own.</summary>
    public required InstalledRoomObjectPictures RoomObjectMaridiaElevatubeVisuals { get; init; }

    /// <summary>The installed pictures of the two-by-two Spore Spawn ceiling clear/crumble draws; their terrain and sequence timing are the game's own.</summary>
    public required InstalledRoomObjectPictures RoomObjectSporeSpawnCeilingVisuals { get; init; }

    /// <summary>The installed pictures of the Samus eater plant; the three-run collision layout is the game's own.</summary>
    public required InstalledRoomObjectPictures RoomObjectSamusEaterVisuals { get; init; }

    /// <summary>The installed picture of the nine-block vertical Botwoon wall clear; its air-block collision remains unchanged.</summary>
    public required InstalledRoomObjectPictures RoomObjectBotwoonWallVisuals { get; init; }

    /// <summary>The installed pictures of Kraid's ceiling and spike frames, distinct from the boss's own body frames and from elevatube owners of the shared draw; block collision and mutation timing are the game's own.</summary>
    public required InstalledRoomObjectPictures RoomObjectKraidVisuals { get; init; }

    /// <summary>The installed pictures of Crocomire's bridge-clear, crumble and invisible-wall frames; the draw runs still own block placement and collision.</summary>
    public required InstalledRoomObjectPictures RoomObjectCrocomireVisuals { get; init; }

    /// <summary>The installed pictures of Mother Brain's fake-death room wall, door, mural rows, fill rows and tube clears; geometry, collision and timing stay with the draws.</summary>
    public required InstalledMotherBrainFakeDeathFrames RoomObjectMotherBrainFakeDeathVisuals { get; init; }

    /// <summary>Collectible art; the terrain retains collision.</summary>
    public required InstalledRoomObjectPictures RoomObjectCollectibleVisuals { get; init; }

    /// <summary>The installed pictures of the bombed reveals, the escape passage, the escape shaft wall and the animals' rescue wall.</summary>
    public required InstalledRoomObjectPictures RoomObjectRevealAndWallVisuals { get; init; }

    /// <summary>The installed pictures of the blocks room-object setups and block reactions write of their own accord (#1580).</summary>
    public required InstalledRoomObjectSetupBlocks RoomObjectSetupBlocks { get; init; }

    /// <summary>Every family's catalog, each listing its pictures under the draws that show them (#2571).</summary>
    internal IReadOnlyList<IRoomObjectFamilyPictures> FamilyPictures =>
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
    ];
}
