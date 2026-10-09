using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Host-selected BG tilemaps, rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public RoomBackgroundTilemapCatalog? RoomBackgroundTilemapArt { get; set; }
}
