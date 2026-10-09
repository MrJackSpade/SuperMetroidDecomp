using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Stores host-provided room palettes outside debugger serialization.</summary>
    [NonSerialized] private RoomStaticPaletteCatalog? roomPaletteArt;

    /// <summary>Host-selected base room palettes, rebound after debugger-state restore.</summary>
    public RoomStaticPaletteCatalog? RoomPaletteArt
    {
        get => roomPaletteArt;
        set => roomPaletteArt = value;
    }
}
