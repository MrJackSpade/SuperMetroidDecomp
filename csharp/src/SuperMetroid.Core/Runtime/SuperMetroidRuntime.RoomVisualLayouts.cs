using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Host-selected visual room layouts, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomVisualLayoutCatalog? RoomVisualLayouts { get; set; }
}
