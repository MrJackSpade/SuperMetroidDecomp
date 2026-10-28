namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The seven frames the Ceres door, the Baby's capsule and Ridley's wings draw onto the rotating floor.
/// Its values are arbitrary; verification's native view names each member's cartridge list (#1415).
/// </summary>
public enum CeresFloorAnimationFrame
{
    /// <summary>The elevator landing platform's light frame.</summary>
    ElevatorLight = 1,
    /// <summary>The elevator landing platform's dark frame.</summary>
    ElevatorDark,
    /// <summary>The Baby capsule's frame zero.</summary>
    BabyFrame0,
    /// <summary>The Baby capsule's frame one, also shown as its fourth phase.</summary>
    BabyFrame1,
    /// <summary>The Baby capsule's frame two.</summary>
    BabyFrame2,
    /// <summary>Ridley's wings' frame zero.</summary>
    WingFrame0,
    /// <summary>Ridley's wings' frame one.</summary>
    WingFrame1,
}
