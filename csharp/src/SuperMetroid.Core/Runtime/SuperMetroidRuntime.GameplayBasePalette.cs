using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Installed colors used at construction and on each room-entry palette restore.</summary>
    [field: NonSerialized]
    public GameplayBasePaletteCatalog? InitialPaletteArt { get; set; }
}
