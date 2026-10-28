using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// Visual state of the room's shot-reactive PLM blocks.
/// </summary>
[ArchitectureReview("context-bag", "The installed PLM visual catalogs a host binds; every member is immutable artwork data, not a service.")]
internal sealed class PlmShotBlockVisuals
{
    /// <summary>Nonserialized visual-only shot-block selection; native collision words remain compiled.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmShotBlockVisuals? RoomPlmShotBlockVisuals { get; set; }

    /// <summary>Nonserialized Grapple-block art; the compiled full level words own collision.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmGrappleBlockVisuals? RoomPlmGrappleBlockVisuals { get; set; }

    /// <summary>Nonserialized station appearance; the compiled level words retain collision.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmStationVisuals? RoomPlmStationVisuals { get; set; }

    /// <summary>Visual-only blue-door cap frames supplied by installed content.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmBlueDoorVisuals? RoomPlmBlueDoorVisuals { get; set; }

    /// <summary>Visual-only colored-door cap frames supplied by installed content.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmColoredDoorVisuals? RoomPlmColoredDoorVisuals { get; set; }

    /// <summary>Visual-only grey caps and shared clear frames supplied by installed content.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmGreyDoorVisuals? RoomPlmGreyDoorVisuals { get; set; }

    /// <summary>Visual-only mirrored eye-door frames supplied by installed content.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmEyeDoorVisuals? RoomPlmEyeDoorVisuals { get; set; }

    /// <summary>Visual-only Mother Brain glass frames supplied by installed content.</summary>
    [field: NonSerialized]
    public RoomPlmMotherBrainGlassVisualCatalog? RoomPlmMotherBrainGlassVisuals { get; set; }

    /// <summary>Visual-only n00b-tube frames supplied by installed content.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmNoobTubeVisuals? RoomPlmNoobTubeVisuals { get; set; }

    /// <summary>Nonserialized gate-block appearance; compiled level words retain collision.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmDownwardGateVisuals? RoomPlmDownwardGateVisuals { get; set; }

    /// <summary>Visual-only elevator frames; platform collision and timing remain compiled.</summary>
    [field: NonSerialized]
    public RoomPlmElevatorPlatformVisualCatalog? RoomPlmElevatorPlatformVisuals { get; set; }

    /// <summary>Nonserialized Mother Brain escape-gate appearance; collision stays compiled.</summary>
    [field: NonSerialized]
    public RoomPlmEscapeGateVisualCatalog? RoomPlmEscapeGateVisuals { get; set; }

    /// <summary>Nonserialized Bomb Torizo hand art; physical draw words stay compiled.</summary>
    [field: NonSerialized]
    public RoomPlmBombTorizoHandVisualCatalog? RoomPlmBombTorizoHandVisuals { get; set; }

    /// <summary>Nonserialized Draygon cannon art; physical draw words stay compiled.</summary>
    [field: NonSerialized]
    public RoomPlmDraygonCannonVisualCatalog? RoomPlmDraygonCannonVisuals { get; set; }

    /// <summary>Nonserialized Chozo statue PLM art; physical draw words stay compiled.</summary>
    [field: NonSerialized]
    public RoomPlmChozoStatueVisualCatalog? RoomPlmChozoStatueVisuals { get; set; }

    /// <summary>Nonserialized linked-block restoration art; collision stays compiled.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmLinkedRestoreVisuals? RoomPlmLinkedRestoreVisuals { get; set; }

    /// <summary>Nonserialized Tourian access-floor art; physical level words stay compiled.</summary>
    [field: NonSerialized]
    public RoomPlmTourianAccessVisuals? RoomPlmTourianAccessVisuals { get; set; }

    /// <summary>Gets or replaces nonserialized Speed Booster block reveal artwork; compiled level words still determine collision and breakability.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the physical draw word's original visual fields.</remarks>
    [field: NonSerialized]
    public InstalledRoomPlmSpeedBoosterVisuals? RoomPlmSpeedBoosterVisuals { get; set; }

    /// <summary>Gets or replaces nonserialized Maridia elevatube artwork for the shared $84:9367 draw, selected by PLM ownership rather than borrowing Kraid's appearance.</summary>
    /// <remarks>The catalog is shared by reference. Null preserves the physical word's original appearance; tube collision and motion remain compiled.</remarks>
    [field: NonSerialized]
    public InstalledRoomPlmMaridiaElevatubeVisuals? RoomPlmMaridiaElevatubeVisuals { get; set; }

    /// <summary>Gets or replaces nonserialized artwork for the two-by-two Spore Spawn ceiling clear/crumble draws, without changing their physical words or sequence timing.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each physical draw word's original visual fields.</remarks>
    [field: NonSerialized]
    public RoomPlmSporeSpawnCeilingVisualCatalog? RoomPlmSporeSpawnCeilingVisuals { get; set; }

    /// <summary>Replaceable plant tiles; the three-run collision layout stays compiled.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmSamusEaterVisuals? RoomPlmSamusEaterVisuals { get; set; }

    /// <summary>Gets or replaces nonserialized appearance for $84:930F's nine-block vertical Botwoon wall clear; its compiled air-block collision remains unchanged.</summary>
    /// <remarks>The catalog is shared by reference. Null uses the physical clear words' original visual fields.</remarks>
    [field: NonSerialized]
    public InstalledRoomPlmBotwoonWallVisuals? RoomPlmBotwoonWallVisuals { get; set; }

    /// <summary>Gets or replaces nonserialized Kraid ceiling/spike PLM artwork, distinct from the boss's private BG2 body maps and from elevatube owners of the shared $84:9367 draw.</summary>
    /// <remarks>The catalog is shared by reference. Null retains physical draw appearance; block collision and mutation timing remain compiled.</remarks>
    [field: NonSerialized]
    public InstalledRoomPlmKraidVisuals? RoomPlmKraidVisuals { get; set; }

    /// <summary>Gets or replaces nonserialized Crocomire arena bridge-clear/crumble and invisible-wall artwork; compiled draw runs still own block placement and collision.</summary>
    /// <remarks>The catalog is shared by reference. Null retains each physical draw word's original visual fields.</remarks>
    [field: NonSerialized]
    public RoomPlmCrocomireVisualCatalog? RoomPlmCrocomireVisuals { get; set; }

    /// <summary>Gets or replaces nonserialized Mother Brain fake-death room wall, door, and background-row artwork, separate from the boss's sprite and palette presentations.</summary>
    /// <remarks>The catalog is shared by reference. Null retains physical draw appearance; compiled run geometry, collision words, and PLM timing remain unchanged.</remarks>
    [field: NonSerialized]
    public RoomPlmMotherBrainFakeDeathVisualCatalog? RoomPlmMotherBrainFakeDeathVisuals { get; set; }

    /// <summary>Nonserialized collectible art; compiled level words retain collision.</summary>
    [field: NonSerialized]
    public InstalledRoomPlmCollectibleVisuals? RoomPlmCollectibleVisuals { get; set; }
}
