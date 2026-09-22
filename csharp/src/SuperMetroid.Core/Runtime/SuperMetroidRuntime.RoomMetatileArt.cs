using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    [NonSerialized] private RoomMetatileCatalog? roomMetatileArt;

    /// <summary>Host-selected visual block compositions, rebound after debugger-state restore.</summary>
    public RoomMetatileCatalog? RoomMetatileArt
    {
        get => roomMetatileArt;
        set => roomMetatileArt = value;
    }
}
