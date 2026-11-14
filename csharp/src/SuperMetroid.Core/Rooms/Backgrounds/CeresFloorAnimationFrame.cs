namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The seven frames the Ceres door, the Baby's capsule and Ridley's wings draw onto the rotating floor.
/// Its values are arbitrary.
/// </summary>
public enum CeresFloorAnimationFrame
{
    /// <summary>The elevator landing platform's light frame.</summary>
    ElevatorLight = 1,
    /// <summary>The elevator landing platform's dark frame.</summary>
    ElevatorDark,
    /// <summary>The Baby in its capsule, squished vertically.</summary>
    BabyVertical,
    /// <summary>The Baby in its capsule, round.</summary>
    BabyRound,
    /// <summary>The Baby in its capsule, squished horizontally.</summary>
    BabyHorizontal,
    /// <summary>Ridley's wings raised.</summary>
    WingsRaised,
    /// <summary>Ridley's wings lowered.</summary>
    WingsLowered,
}
