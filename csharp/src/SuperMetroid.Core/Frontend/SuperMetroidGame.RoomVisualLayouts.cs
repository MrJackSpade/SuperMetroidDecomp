using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomVisualLayoutCatalog? roomVisualLayouts;

    /// <summary>Attaches installed visual room layouts after construction or state restoration.</summary>
    public void BindRoomVisualLayouts(RoomVisualLayoutCatalog? catalog)
    {
        roomVisualLayouts = catalog;
        if (runtime is not null) runtime.RoomVisualLayouts = catalog;
    }
}
