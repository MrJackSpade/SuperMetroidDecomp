using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Visual-only shot-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmShotBlockVisualCatalog? RoomPlmShotBlockVisuals
    {
        get => Plms.ShotBlockVisuals;
        set => Plms.ShotBlockVisuals = value;
    }
}
