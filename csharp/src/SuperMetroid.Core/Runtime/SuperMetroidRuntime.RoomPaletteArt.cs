using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Host-selected base room palettes, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomStaticPaletteCatalog? RoomPaletteArt { get; set; }
}
