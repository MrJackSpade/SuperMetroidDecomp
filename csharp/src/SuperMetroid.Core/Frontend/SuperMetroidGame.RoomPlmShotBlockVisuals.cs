using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomPlmShotBlockVisualCatalog? roomPlmShotBlockVisuals;
    [NonSerialized] private RoomPlmGrappleBlockVisualCatalog? roomPlmGrappleBlockVisuals;
    [NonSerialized] private RoomPlmStationVisualCatalog? roomPlmStationVisuals;

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
}
