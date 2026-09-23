using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    [NonSerialized] private RoomVisualLayoutCatalog? roomVisualLayouts;

    /// <summary>Host-selected visual room layouts, rebound after debugger-state restore.</summary>
    public RoomVisualLayoutCatalog? RoomVisualLayouts
    {
        get => roomVisualLayouts;
        set => roomVisualLayouts = value;
    }
}
