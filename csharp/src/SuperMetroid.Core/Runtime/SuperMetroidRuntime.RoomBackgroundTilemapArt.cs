using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    [NonSerialized] private RoomBackgroundTilemapCatalog? roomBackgroundTilemapArt;

    /// <summary>Host-selected BG tilemaps, rebound after debugger-state restoration.</summary>
    public RoomBackgroundTilemapCatalog? RoomBackgroundTilemapArt
    {
        get => roomBackgroundTilemapArt;
        set => roomBackgroundTilemapArt = value;
    }
}
