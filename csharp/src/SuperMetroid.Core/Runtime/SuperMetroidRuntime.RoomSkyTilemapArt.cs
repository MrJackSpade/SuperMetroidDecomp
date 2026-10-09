using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Stores installed scrolling-sky tilemaps outside debugger serialization.</summary>
    [NonSerialized] private RoomSkyTilemapCatalog? roomSkyTilemapArt;

    /// <summary>Installed scrolling-sky pages, rebound after debugger-state restoration.</summary>
    public RoomSkyTilemapCatalog? RoomSkyTilemapArt
    {
        get => roomSkyTilemapArt;
        set => roomSkyTilemapArt = value;
    }
}
