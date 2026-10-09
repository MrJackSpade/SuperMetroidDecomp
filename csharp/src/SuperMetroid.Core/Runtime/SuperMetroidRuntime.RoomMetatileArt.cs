using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Host-selected visual block compositions, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomMetatileCatalog? RoomMetatileArt { get; set; }
}
