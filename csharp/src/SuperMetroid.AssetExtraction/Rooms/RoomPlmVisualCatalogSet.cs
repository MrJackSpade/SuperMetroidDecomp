using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Every room PLM visual catalog whose contents form the PLM presentation identity (#633).</summary>
[ArchitectureReview("context-bag", "The extracted PLM visual catalogs, one per object family; every member is immutable artwork data, not a service.")]
public sealed record RoomPlmVisualCatalogSet
{
    /// <summary>Shot block.</summary>
    public required InstalledRoomPlmShotBlockVisuals ShotBlock { get; init; }

    /// <summary>Grapple block.</summary>
    public required InstalledRoomPlmGrappleBlockVisuals GrappleBlock { get; init; }

    /// <summary>Station.</summary>
    public required InstalledRoomPlmStationVisuals Station { get; init; }

    /// <summary>Blue door.</summary>
    public required InstalledRoomPlmBlueDoorVisuals BlueDoor { get; init; }

    /// <summary>Colored door.</summary>
    public required InstalledRoomPlmColoredDoorVisuals ColoredDoor { get; init; }

    /// <summary>Grey door.</summary>
    public required InstalledRoomPlmGreyDoorVisuals GreyDoor { get; init; }

    /// <summary>Eye door.</summary>
    public required InstalledRoomPlmEyeDoorVisuals EyeDoor { get; init; }

    /// <summary>Mother brain glass.</summary>
    public required RoomPlmMotherBrainGlassVisualCatalog MotherBrainGlass { get; init; }

    /// <summary>Noob tube.</summary>
    public required InstalledRoomPlmNoobTubeVisuals NoobTube { get; init; }

    /// <summary>Downward gate.</summary>
    public required InstalledRoomPlmDownwardGateVisuals DownwardGate { get; init; }

    /// <summary>Elevator platform.</summary>
    public required RoomPlmElevatorPlatformVisualCatalog ElevatorPlatform { get; init; }

    /// <summary>Escape gate.</summary>
    public required RoomPlmEscapeGateVisualCatalog EscapeGate { get; init; }

    /// <summary>Bomb torizo hand.</summary>
    public required RoomPlmBombTorizoHandVisualCatalog BombTorizoHand { get; init; }

    /// <summary>Draygon cannon.</summary>
    public required RoomPlmDraygonCannonVisualCatalog DraygonCannon { get; init; }

    /// <summary>Chozo statue.</summary>
    public required RoomPlmChozoStatueVisualCatalog ChozoStatue { get; init; }

    /// <summary>Linked restore.</summary>
    public required InstalledRoomPlmLinkedRestoreVisuals LinkedRestore { get; init; }

    /// <summary>Tourian access.</summary>
    public required RoomPlmTourianAccessVisuals TourianAccess { get; init; }

    /// <summary>Speed booster.</summary>
    public required InstalledRoomPlmSpeedBoosterVisuals SpeedBooster { get; init; }

    /// <summary>Maridia elevatube.</summary>
    public required InstalledRoomPlmMaridiaElevatubeVisuals MaridiaElevatube { get; init; }

    /// <summary>Spore spawn ceiling.</summary>
    public required RoomPlmSporeSpawnCeilingVisualCatalog SporeSpawnCeiling { get; init; }

    /// <summary>Samus eater.</summary>
    public required InstalledRoomPlmSamusEaterVisuals SamusEater { get; init; }

    /// <summary>Botwoon wall.</summary>
    public required InstalledRoomPlmBotwoonWallVisuals BotwoonWall { get; init; }

    /// <summary>Kraid.</summary>
    public required InstalledRoomPlmKraidVisuals Kraid { get; init; }

    /// <summary>Crocomire.</summary>
    public required RoomPlmCrocomireVisualCatalog Crocomire { get; init; }

    /// <summary>Mother brain fake death.</summary>
    public required RoomPlmMotherBrainFakeDeathVisualCatalog MotherBrainFakeDeath { get; init; }

    /// <summary>Collectible.</summary>
    public required InstalledRoomPlmCollectibleVisuals Collectible { get; init; }
}
