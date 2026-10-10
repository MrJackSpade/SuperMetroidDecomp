using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Installed visual block catalog retained for binding to the active runtime after creation or restoration.</summary>
    [NonSerialized] private RoomMetatileCatalog? roomMetatileArt;

    /// <summary>Attaches installed visual block compositions after construction or state restoration.</summary>
    public void BindRoomMetatileArt(RoomMetatileCatalog? catalog)
    {
        roomMetatileArt = catalog;
        if (runtime is not null) runtime.RoomMetatileArt = catalog;
    }
}
