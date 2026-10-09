using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Installed scrolling-sky pages, rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public RoomSkyTilemapCatalog? RoomSkyTilemapArt { get; set; }
}
