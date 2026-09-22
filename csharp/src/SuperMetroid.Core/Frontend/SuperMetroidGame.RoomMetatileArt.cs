using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomMetatileCatalog? roomMetatileArt;

    /// <summary>Attaches installed visual block compositions after construction or state restoration.</summary>
    public void BindRoomMetatileArt(RoomMetatileCatalog? catalog)
    {
        roomMetatileArt = catalog;
        if (runtime is not null) runtime.RoomMetatileArt = catalog;
    }
}
