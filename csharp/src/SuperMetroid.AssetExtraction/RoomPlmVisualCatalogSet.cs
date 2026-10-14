using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Every room PLM visual catalog whose contents form the PLM presentation identity (#633).</summary>
public sealed record RoomPlmVisualCatalogSet
{
    /// <summary>Shot block.</summary>
    public required RoomPlmShotBlockVisualCatalog ShotBlock { get; init; }

    /// <summary>Grapple block.</summary>
    public required RoomPlmGrappleBlockVisualCatalog GrappleBlock { get; init; }

    /// <summary>Station.</summary>
    public required RoomPlmStationVisualCatalog Station { get; init; }

    /// <summary>Blue door.</summary>
    public required RoomPlmBlueDoorVisualCatalog BlueDoor { get; init; }

    /// <summary>Colored door.</summary>
    public required RoomPlmColoredDoorVisualCatalog ColoredDoor { get; init; }

    /// <summary>Grey door.</summary>
    public required RoomPlmGreyDoorVisualCatalog GreyDoor { get; init; }

    /// <summary>Eye door.</summary>
    public required RoomPlmEyeDoorVisualCatalog EyeDoor { get; init; }

    /// <summary>Mother brain glass.</summary>
    public required RoomPlmMotherBrainGlassVisualCatalog MotherBrainGlass { get; init; }

    /// <summary>Noob tube.</summary>
    public required RoomPlmNoobTubeVisualCatalog NoobTube { get; init; }

    /// <summary>Downward gate.</summary>
    public required RoomPlmDownwardGateVisualCatalog DownwardGate { get; init; }

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
    public required RoomPlmLinkedRestoreVisualCatalog LinkedRestore { get; init; }

    /// <summary>Tourian access.</summary>
    public required RoomPlmTourianAccessVisualCatalog TourianAccess { get; init; }

    /// <summary>Speed booster.</summary>
    public required RoomPlmSpeedBoosterVisualCatalog SpeedBooster { get; init; }

    /// <summary>Maridia elevatube.</summary>
    public required RoomPlmMaridiaElevatubeVisualCatalog MaridiaElevatube { get; init; }

    /// <summary>Spore spawn ceiling.</summary>
    public required RoomPlmSporeSpawnCeilingVisualCatalog SporeSpawnCeiling { get; init; }

    /// <summary>Samus eater.</summary>
    public required RoomPlmSamusEaterVisualCatalog SamusEater { get; init; }

    /// <summary>Botwoon wall.</summary>
    public required RoomPlmBotwoonWallVisualCatalog BotwoonWall { get; init; }

    /// <summary>Kraid.</summary>
    public required RoomPlmKraidVisualCatalog Kraid { get; init; }

    /// <summary>Crocomire.</summary>
    public required RoomPlmCrocomireVisualCatalog Crocomire { get; init; }

    /// <summary>Mother brain fake death.</summary>
    public required RoomPlmMotherBrainFakeDeathVisualCatalog MotherBrainFakeDeath { get; init; }

    /// <summary>Collectible.</summary>
    public required RoomPlmCollectibleVisualCatalog Collectible { get; init; }

    /// <summary>Dynamic collectible art.</summary>
    public required RoomPlmDynamicCollectibleArtCatalog DynamicCollectibleArt { get; init; }
}
