namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The actors a Ceres room's turning floor shows (#1626, #2573), each one of its own pictures at a time, drawn with the
/// floor's transform over it: they are not the floor's, which holds only the floor.
/// </summary>
public enum CeresFloorActor
{
    /// <summary>The elevator room's landing platform, alternating its light and dark frames.</summary>
    ElevatorPlatform,
    /// <summary>The Baby's capsule Ridley carries off, cycling its frames during the getaway.</summary>
    BabyCapsule,
    /// <summary>Ridley's wings, beating during the getaway.</summary>
    RidleyWings,
}
