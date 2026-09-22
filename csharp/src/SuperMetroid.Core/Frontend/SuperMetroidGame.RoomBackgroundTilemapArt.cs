using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private RoomBackgroundTilemapCatalog? roomBackgroundTilemapArt;

    /// <summary>Attaches installed library-background visuals after construction or state restore.</summary>
    public void BindRoomBackgroundTilemapArt(RoomBackgroundTilemapCatalog? catalog)
    {
        roomBackgroundTilemapArt = catalog;
        if (runtime is not null) runtime.RoomBackgroundTilemapArt = catalog;
    }
}
