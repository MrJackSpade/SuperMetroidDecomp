using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// Visual state of the room's shot-reactive room-object blocks.
/// </summary>
[ArchitectureReview("context-bag", "The installed room-object visual catalogs a host binds; every member is immutable artwork data, not a service.")]
internal sealed class RoomObjectVisuals
{
    /// <summary>Visual-only shot-block selection; collision words remain compiled.</summary>
    public InstalledRoomObjectShotBlockVisuals? RoomObjectShotBlockVisuals { get; set; }

    /// <summary>Grapple-block art; the compiled full level words own collision.</summary>
    public InstalledRoomObjectGrappleBlockVisuals? RoomObjectGrappleBlockVisuals { get; set; }

    /// <summary>Station appearance; the compiled level words retain collision.</summary>
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

    /// <summary>Gate-block appearance; compiled level words retain collision.</summary>
    public InstalledRoomObjectDownwardGateVisuals? RoomObjectDownwardGateVisuals { get; set; }

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    public InstalledRoomObjectElevatorPlatformVisuals? RoomObjectElevatorPlatformVisuals { get; set; }

    /// <summary>The installed pictures of Mother Brain's escape-gate frames; collision stays with the draws.</summary>
    public InstalledEscapeGateFrames? RoomObjectEscapeGateVisuals { get; set; }

    /// <summary>Bomb Torizo hand art; physical draw words stay compiled.</summary>
    public InstalledRoomObjectBombTorizoHandVisuals? RoomObjectBombTorizoHandVisuals { get; set; }

    /// <summary>The installed pictures of Draygon's cannon frames; collision stays with the draws.</summary>
    public InstalledDraygonCannonFrames? RoomObjectDraygonCannonVisuals { get; set; }

    /// <summary>Chozo statue room-object art; physical draw words stay compiled.</summary>
    public InstalledRoomObjectChozoStatueVisuals? RoomObjectChozoStatueVisuals { get; set; }

    /// <summary>Linked-block restoration art; collision stays compiled.</summary>
    public InstalledRoomObjectLinkedRestoreVisuals? RoomObjectLinkedRestoreVisuals { get; set; }

    /// <summary>Tourian access-floor art; physical level words stay compiled.</summary>
    public RoomObjectTourianAccessVisuals? RoomObjectTourianAccessVisuals { get; set; }

    /// <summary>Gets or replaces Speed Booster block reveal artwork; compiled level words still determine collision and breakability.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the physical draw word's original visual fields.</remarks>
    public InstalledRoomObjectSpeedBoosterVisuals? RoomObjectSpeedBoosterVisuals { get; set; }

    /// <summary>Gets or replaces Maridia elevatube artwork for the shared elevatube draw, selected by room-object ownership rather than borrowing Kraid's appearance.</summary>
    /// <remarks>The catalog is shared by reference. Null preserves the physical word's original appearance; tube collision and motion remain compiled.</remarks>
    public InstalledRoomObjectMaridiaElevatubeVisuals? RoomObjectMaridiaElevatubeVisuals { get; set; }

    /// <summary>Gets or replaces artwork for the two-by-two Spore Spawn ceiling clear/crumble draws, without changing their physical words or sequence timing.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each physical draw word's original visual fields.</remarks>
    public InstalledRoomObjectSporeSpawnCeilingVisuals? RoomObjectSporeSpawnCeilingVisuals { get; set; }

    /// <summary>Replaceable plant tiles; the three-run collision layout stays compiled.</summary>
    public InstalledRoomObjectSamusEaterVisuals? RoomObjectSamusEaterVisuals { get; set; }

    /// <summary>Gets or replaces appearance for the nine-block vertical Botwoon wall clear; its compiled air-block collision remains unchanged.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the physical clear words' original visual fields.</remarks>
    public InstalledRoomObjectBotwoonWallVisuals? RoomObjectBotwoonWallVisuals { get; set; }

    /// <summary>Gets or replaces Kraid ceiling/spike room-object artwork, distinct from the boss's private body maps and from elevatube owners of the shared elevatube draw.</summary>
    /// <remarks>The catalog is shared by reference. Null retains physical draw appearance; block collision and mutation timing remain compiled.</remarks>
    public InstalledRoomObjectKraidVisuals? RoomObjectKraidVisuals { get; set; }

    /// <summary>Gets or replaces Crocomire arena bridge-clear/crumble and invisible-wall artwork; compiled draw runs still own block placement and collision.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each physical draw word's original visual fields.</remarks>
    public InstalledRoomObjectCrocomireVisuals? RoomObjectCrocomireVisuals { get; set; }

    /// <summary>The installed pictures of Mother Brain's fake-death room wall, door, mural rows, fill rows and tube clears; geometry, collision and timing stay with the draws.</summary>
    public InstalledMotherBrainFakeDeathFrames? RoomObjectMotherBrainFakeDeathVisuals { get; set; }

    /// <summary>Collectible art; compiled level words retain collision.</summary>
    public InstalledRoomObjectCollectibleVisuals? RoomObjectCollectibleVisuals { get; set; }

    /// <summary>The installed pictures of the bombed reveals, the escape passage, the escape shaft wall and the animals' rescue wall.</summary>
    public InstalledRoomObjectRevealAndWallVisuals? RoomObjectRevealAndWallVisuals { get; set; }

    /// <summary>The installed pictures of the blocks room-object setups and block reactions write of their own accord (#1580).</summary>
    public InstalledRoomObjectSetupBlocks? RoomObjectSetupBlocks { get; set; }
}
