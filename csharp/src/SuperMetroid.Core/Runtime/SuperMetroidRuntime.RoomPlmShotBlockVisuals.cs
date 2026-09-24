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

    /// <summary>Visual-only Grapple-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmGrappleBlockVisualCatalog? RoomPlmGrappleBlockVisuals
    {
        get => Plms.GrappleBlockVisuals;
        set => Plms.GrappleBlockVisuals = value;
    }

    /// <summary>Visual-only station resources; reattached after debugger-state restoration.</summary>
    public RoomPlmStationVisualCatalog? RoomPlmStationVisuals
    {
        get => Plms.StationVisuals;
        set => Plms.StationVisuals = value;
    }

    /// <summary>Visual-only blue-door cap resources; reattached after debugger-state restoration.</summary>
    public RoomPlmBlueDoorVisualCatalog? RoomPlmBlueDoorVisuals
    {
        get => Plms.BlueDoorVisuals;
        set => Plms.BlueDoorVisuals = value;
    }

    /// <summary>Visual-only gate-block resources; reattached after debugger-state restoration.</summary>
    public RoomPlmDownwardGateVisualCatalog? RoomPlmDownwardGateVisuals
    {
        get => Plms.DownwardGateVisuals;
        set => Plms.DownwardGateVisuals = value;
    }

    /// <summary>Visual-only collectible resources; reattached after debugger-state restoration.</summary>
    public RoomPlmCollectibleVisualCatalog? RoomPlmCollectibleVisuals
    {
        get => Plms.CollectibleVisuals;
        set => Plms.CollectibleVisuals = value;
    }
}
