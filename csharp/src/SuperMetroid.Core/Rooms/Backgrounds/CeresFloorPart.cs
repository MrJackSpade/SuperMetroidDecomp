namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>The animated parts of a Ceres room's turning floor, each showing one of its own frames at a time (#1626).</summary>
public enum CeresFloorPart
{
    /// <summary>The elevator room's landing platform, alternating its light and dark frames.</summary>
    ElevatorPlatform,
    /// <summary>The Baby's capsule Ridley carries off, cycling its frames during the getaway.</summary>
    BabyCapsule,
    /// <summary>Ridley's wings, beating during the getaway.</summary>
    RidleyWings,
}
