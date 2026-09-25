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
    [NonSerialized] private RoomPlmDownwardGateVisualCatalog? roomPlmDownwardGateVisuals;
    [NonSerialized] private RoomPlmCollectibleVisualCatalog? roomPlmCollectibleVisuals;

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

    /// <summary>Binds installed downward-gate block art at startup and after state restoration.</summary>
    public void BindRoomPlmDownwardGateVisuals(RoomPlmDownwardGateVisualCatalog? catalog)
    {
        roomPlmDownwardGateVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmDownwardGateVisuals = catalog;
    }

    /// <summary>Binds installed collectible art at startup and after state restoration.</summary>
    public void BindRoomPlmCollectibleVisuals(RoomPlmCollectibleVisualCatalog? catalog)
    {
        roomPlmCollectibleVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmCollectibleVisuals = catalog;
    }
}
