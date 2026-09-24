using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomPlmShotBlockVisualCatalog? roomPlmShotBlockVisuals;

    /// <summary>Binds installed shot-block visuals at startup and after state restoration.</summary>
    public void BindRoomPlmShotBlockVisuals(RoomPlmShotBlockVisualCatalog? catalog)
    {
        roomPlmShotBlockVisuals = catalog;
        if (runtime is not null) runtime.RoomPlmShotBlockVisuals = catalog;
    }
}
