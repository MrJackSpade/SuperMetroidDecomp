namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The pictures of the actors Ceres's turning floor shows (#2573): the elevator's landing platform, the Baby's capsule and
/// Ridley's wings, each drawn with the floor's transform over it, at its actor's place on the floor. Its values are
/// arbitrary.
/// </summary>
public enum CeresFloorActorPicture
{
    /// <summary>The elevator landing platform's light frame.</summary>
    ElevatorLight,
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
