using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomPlmShotBlockVisualCatalog? roomPlmShotBlockVisuals;
    [NonSerialized] private RoomPlmGrappleBlockVisualCatalog? roomPlmGrappleBlockVisuals;
    [NonSerialized] private RoomPlmStationVisualCatalog? roomPlmStationVisuals;
    [NonSerialized] private RoomPlmBlueDoorVisualCatalog? roomPlmBlueDoorVisuals;
    [NonSerialized] private RoomPlmColoredDoorVisualCatalog? roomPlmColoredDoorVisuals;
    [NonSerialized] private RoomPlmGreyDoorVisualCatalog? roomPlmGreyDoorVisuals;
    [NonSerialized] private RoomPlmEyeDoorVisualCatalog? roomPlmEyeDoorVisuals;
    [NonSerialized] private RoomPlmMotherBrainGlassVisualCatalog? roomPlmMotherBrainGlassVisuals;
    [NonSerialized] private RoomPlmNoobTubeVisualCatalog? roomPlmNoobTubeVisuals;
    [NonSerialized] private RoomPlmDownwardGateVisualCatalog? roomPlmDownwardGateVisuals;
    [NonSerialized] private RoomPlmEscapeGateVisualCatalog? roomPlmEscapeGateVisuals;
    [NonSerialized] private RoomPlmBombTorizoHandVisualCatalog? roomPlmBombTorizoHandVisuals;
    [NonSerialized] private RoomPlmDraygonCannonVisualCatalog? roomPlmDraygonCannonVisuals;
    [NonSerialized] private RoomPlmChozoStatueVisualCatalog? roomPlmChozoStatueVisuals;
    [NonSerialized] private RoomPlmLinkedRestoreVisualCatalog? roomPlmLinkedRestoreVisuals;
    [NonSerialized] private RoomPlmTourianAccessVisualCatalog? roomPlmTourianAccessVisuals;
    [NonSerialized] private RoomPlmSpeedBoosterVisualCatalog? roomPlmSpeedBoosterVisuals;
    [NonSerialized] private RoomPlmMaridiaElevatubeVisualCatalog? roomPlmMaridiaElevatubeVisuals;
    [NonSerialized] private RoomPlmSporeSpawnCeilingVisualCatalog? roomPlmSporeSpawnCeilingVisuals;
    [NonSerialized] private RoomPlmBotwoonWallVisualCatalog? roomPlmBotwoonWallVisuals;
    [NonSerialized] private RoomPlmKraidVisualCatalog? roomPlmKraidVisuals;
    [NonSerialized] private RoomPlmCrocomireVisualCatalog? roomPlmCrocomireVisuals;
    [NonSerialized] private RoomPlmCollectibleVisualCatalog? roomPlmCollectibleVisuals;
    [NonSerialized] private RoomPlmDynamicCollectibleArtCatalog? roomPlmDynamicCollectibleArt;

    /// <summary>Binds installed shot-block visuals at startup and after state restoration.</summary>
    public void BindRoomPlmShotBlockVisuals(RoomPlmShotBlockVisualCatalog? catalog)
    {
        roomPlmShotBlockVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmShotBlockVisuals = catalog;
    }

    /// <summary>Binds installed Grapple-block art at startup and after state restoration.</summary>
    public void BindRoomPlmGrappleBlockVisuals(RoomPlmGrappleBlockVisualCatalog? catalog)
    {
        roomPlmGrappleBlockVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmGrappleBlockVisuals = catalog;
    }

    /// <summary>Binds installed station art at startup and after state restoration.</summary>
    public void BindRoomPlmStationVisuals(RoomPlmStationVisualCatalog? catalog)
    {
        roomPlmStationVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmStationVisuals = catalog;
    }

    /// <summary>Binds installed blue-door cap art at startup and after state restoration.</summary>
    public void BindRoomPlmBlueDoorVisuals(RoomPlmBlueDoorVisualCatalog? catalog)
    {
        roomPlmBlueDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmBlueDoorVisuals = catalog;
    }

    /// <summary>Binds installed colored-door cap art at startup and after state restoration.</summary>
    public void BindRoomPlmColoredDoorVisuals(RoomPlmColoredDoorVisualCatalog? catalog)
    {
        roomPlmColoredDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmColoredDoorVisuals = catalog;
    }

    /// <summary>Binds installed grey-door and shared clear-cap art after state restoration.</summary>
    public void BindRoomPlmGreyDoorVisuals(RoomPlmGreyDoorVisualCatalog? catalog)
    {
        roomPlmGreyDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmGreyDoorVisuals = catalog;
    }

    /// <summary>Binds installed mirrored eye-door art after state restoration.</summary>
    public void BindRoomPlmEyeDoorVisuals(RoomPlmEyeDoorVisualCatalog? catalog)
    {
        roomPlmEyeDoorVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmEyeDoorVisuals = catalog;
    }

    /// <summary>Binds installed Mother Brain glass art after state restoration.</summary>
    public void BindRoomPlmMotherBrainGlassVisuals(
        RoomPlmMotherBrainGlassVisualCatalog? catalog)
    {
        roomPlmMotherBrainGlassVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmMotherBrainGlassVisuals = catalog;
    }

    /// <summary>Binds installed n00b-tube art after state restoration.</summary>
    public void BindRoomPlmNoobTubeVisuals(RoomPlmNoobTubeVisualCatalog? catalog)
    {
        roomPlmNoobTubeVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmNoobTubeVisuals = catalog;
    }

    /// <summary>Binds installed downward-gate block art at startup and after state restoration.</summary>
    public void BindRoomPlmDownwardGateVisuals(RoomPlmDownwardGateVisualCatalog? catalog)
    {
        roomPlmDownwardGateVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmDownwardGateVisuals = catalog;
    }

    /// <summary>Binds installed escape-gate art at startup and after state restoration.</summary>
    public void BindRoomPlmEscapeGateVisuals(RoomPlmEscapeGateVisualCatalog? catalog)
    {
        roomPlmEscapeGateVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmEscapeGateVisuals = catalog;
    }

    /// <summary>Binds installed Bomb Torizo hand art at startup and after state restoration.</summary>
    public void BindRoomPlmBombTorizoHandVisuals(RoomPlmBombTorizoHandVisualCatalog? catalog)
    {
        roomPlmBombTorizoHandVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmBombTorizoHandVisuals = catalog;
    }

    /// <summary>Binds installed Draygon cannon art at startup and after state restoration.</summary>
    public void BindRoomPlmDraygonCannonVisuals(RoomPlmDraygonCannonVisualCatalog? catalog)
    {
        roomPlmDraygonCannonVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmDraygonCannonVisuals = catalog;
    }

    /// <summary>Binds installed Chozo statue terrain art after state restoration.</summary>
    public void BindRoomPlmChozoStatueVisuals(RoomPlmChozoStatueVisualCatalog? catalog)
    {
        roomPlmChozoStatueVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmChozoStatueVisuals = catalog;
    }

    /// <summary>Binds linked bomb/contact restore art at startup and after state restoration.</summary>
    public void BindRoomPlmLinkedRestoreVisuals(RoomPlmLinkedRestoreVisualCatalog? catalog)
    {
        roomPlmLinkedRestoreVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmLinkedRestoreVisuals = catalog;
    }

    /// <summary>Binds Tourian access-floor art at startup and after state restoration.</summary>
    public void BindRoomPlmTourianAccessVisuals(RoomPlmTourianAccessVisualCatalog? catalog)
    {
        roomPlmTourianAccessVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmTourianAccessVisuals = catalog;
    }

    /// <summary>Binds Speed Booster bomb-reveal art after startup or state restoration.</summary>
    public void BindRoomPlmSpeedBoosterVisuals(RoomPlmSpeedBoosterVisualCatalog? catalog)
    {
        roomPlmSpeedBoosterVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmSpeedBoosterVisuals = catalog;
    }

    /// <summary>Binds the installed Maridia elevatube tile after startup or state restoration.</summary>
    public void BindRoomPlmMaridiaElevatubeVisuals(
        RoomPlmMaridiaElevatubeVisualCatalog? catalog)
    {
        roomPlmMaridiaElevatubeVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmMaridiaElevatubeVisuals = catalog;
    }

    /// <summary>Binds Spore Spawn ceiling art after startup or state restoration.</summary>
    public void BindRoomPlmSporeSpawnCeilingVisuals(
        RoomPlmSporeSpawnCeilingVisualCatalog? catalog)
    {
        roomPlmSporeSpawnCeilingVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmSporeSpawnCeilingVisuals = catalog;
    }

    /// <summary>Binds Botwoon wall-clear art after startup or state restoration.</summary>
    public void BindRoomPlmBotwoonWallVisuals(
        RoomPlmBotwoonWallVisualCatalog? catalog)
    {
        roomPlmBotwoonWallVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmBotwoonWallVisuals = catalog;
    }

    /// <summary>Binds Kraid ceiling/spike appearance after startup or state restoration.</summary>
    public void BindRoomPlmKraidVisuals(RoomPlmKraidVisualCatalog? catalog)
    {
        roomPlmKraidVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmKraidVisuals = catalog;
    }

    /// <summary>Binds Crocomire arena appearance after startup or state restoration.</summary>
    public void BindRoomPlmCrocomireVisuals(RoomPlmCrocomireVisualCatalog? catalog)
    {
        roomPlmCrocomireVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmCrocomireVisuals = catalog;
    }

    /// <summary>Binds installed collectible art at startup and after state restoration.</summary>
    public void BindRoomPlmCollectibleVisuals(RoomPlmCollectibleVisualCatalog? catalog)
    {
        roomPlmCollectibleVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmCollectibleVisuals = catalog;
    }

    /// <summary>Binds installed permanent-item PNG and palette art after startup or state restore.</summary>
    public void BindRoomPlmDynamicCollectibleArt(
        RoomPlmDynamicCollectibleArtCatalog? catalog)
    {
        roomPlmDynamicCollectibleArt = catalog;
        if (runtime is not null) runtime.RoomPlmDynamicCollectibleArt = catalog;
    }
}
