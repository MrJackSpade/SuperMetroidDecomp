using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Installed scrolling-sky tilemaps retained for runtime binding and rebound after state restoration.</summary>
    [NonSerialized] private RoomSkyTilemapCatalog? roomSkyTilemapArt;

    /// <summary>Attaches installed scrolling-sky pages after construction or state restoration.</summary>
    public void BindRoomSkyTilemapArt(RoomSkyTilemapCatalog? catalog)
    {
        roomSkyTilemapArt = catalog;
        if (runtime is not null) runtime.RoomSkyTilemapArt = catalog;
    }
}
