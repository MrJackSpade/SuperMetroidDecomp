using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Stores host-provided visual room layouts outside debugger serialization.</summary>
    [NonSerialized] private RoomVisualLayoutCatalog? roomVisualLayouts;

    /// <summary>Host-selected visual room layouts, rebound after debugger-state restore.</summary>
    public RoomVisualLayoutCatalog? RoomVisualLayouts
    {
        get => roomVisualLayouts;
        set => roomVisualLayouts = value;
    }
}
