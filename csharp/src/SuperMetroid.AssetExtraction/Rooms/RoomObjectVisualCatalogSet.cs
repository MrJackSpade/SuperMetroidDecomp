using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Every room PLM visual catalog whose contents form the PLM presentation identity (#633).</summary>
[ArchitectureReview("context-bag", "The extracted PLM visual catalogs, one per object family; every member is immutable artwork data, not a service.")]
public sealed record RoomObjectVisualCatalogSet
{
    /// <summary>Shot block.</summary>
    public required InstalledRoomObjectShotBlockVisuals ShotBlock { get; init; }

    /// <summary>Grapple block.</summary>
    public required InstalledRoomObjectGrappleBlockVisuals GrappleBlock { get; init; }

    /// <summary>Station.</summary>
    public required InstalledRoomObjectStationVisuals Station { get; init; }

    /// <summary>Blue door.</summary>
    public required InstalledRoomObjectBlueDoorVisuals BlueDoor { get; init; }

    /// <summary>Colored door.</summary>
    public required InstalledRoomObjectColoredDoorVisuals ColoredDoor { get; init; }

    /// <summary>Grey door.</summary>
    public required InstalledRoomObjectGreyDoorVisuals GreyDoor { get; init; }

    /// <summary>Eye door.</summary>
    public required InstalledRoomObjectEyeDoorVisuals EyeDoor { get; init; }

    /// <summary>Mother brain glass.</summary>
    public required InstalledMotherBrainGlassFrames MotherBrainGlass { get; init; }

    /// <summary>Noob tube.</summary>
    public required InstalledRoomObjectNoobTubeVisuals NoobTube { get; init; }

    /// <summary>Downward gate.</summary>
    public required InstalledRoomObjectDownwardGateVisuals DownwardGate { get; init; }

    /// <summary>Elevator platform.</summary>
    public required InstalledRoomObjectElevatorPlatformVisuals ElevatorPlatform { get; init; }

    /// <summary>Escape gate.</summary>
    public required InstalledEscapeGateFrames EscapeGate { get; init; }

    /// <summary>Bomb torizo hand.</summary>
    public required InstalledRoomObjectBombTorizoHandVisuals BombTorizoHand { get; init; }

    /// <summary>Draygon cannon.</summary>
    public required InstalledDraygonCannonFrames DraygonCannon { get; init; }

    /// <summary>Chozo statue.</summary>
    public required InstalledRoomObjectChozoStatueVisuals ChozoStatue { get; init; }

    /// <summary>Linked restore.</summary>
    public required InstalledRoomObjectLinkedRestoreVisuals LinkedRestore { get; init; }

    /// <summary>Tourian access.</summary>
    public required RoomObjectTourianAccessVisuals TourianAccess { get; init; }

    /// <summary>Speed booster.</summary>
    public required InstalledRoomObjectSpeedBoosterVisuals SpeedBooster { get; init; }

    /// <summary>Maridia elevatube.</summary>
    public required InstalledRoomObjectMaridiaElevatubeVisuals MaridiaElevatube { get; init; }

    /// <summary>Spore spawn ceiling.</summary>
    public required InstalledRoomObjectSporeSpawnCeilingVisuals SporeSpawnCeiling { get; init; }

    /// <summary>Samus eater.</summary>
    public required InstalledRoomObjectSamusEaterVisuals SamusEater { get; init; }

    /// <summary>Botwoon wall.</summary>
    public required InstalledRoomObjectBotwoonWallVisuals BotwoonWall { get; init; }

    /// <summary>Kraid.</summary>
    public required InstalledRoomObjectKraidVisuals Kraid { get; init; }

    /// <summary>Crocomire.</summary>
    public required InstalledRoomObjectCrocomireVisuals Crocomire { get; init; }

    /// <summary>Mother brain fake death.</summary>
    public required InstalledMotherBrainFakeDeathFrames MotherBrainFakeDeath { get; init; }

    /// <summary>Collectible.</summary>
    public required InstalledRoomObjectCollectibleVisuals Collectible { get; init; }

    /// <summary>Bombed reveals, escape passage, escape shaft wall and rescue wall.</summary>
    public required InstalledRoomObjectRevealAndWallVisuals RevealAndWall { get; init; }
}
