namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The measures of Samus's collision body shared by every pose; each pose's own vertical radius is its definition's
/// (<see cref="SamusPoseDefinitions"/>).
/// </summary>
internal static partial class SamusPoseCollisionDefinitions
{
    /// <summary>
    /// The smallest vertical radius of a body that is not a ball, in pixels: a body that cannot grow into a larger pose
    /// keeps its ball pose below it and crouches from it.
    /// </summary>
    internal const int SmallestUprightYRadius = 8;

    /// <summary>Every pose's horizontal collision radius, in pixels: Samus is five pixels wide either side of her centre.</summary>
    internal const int HorizontalRadius = 5;

    /// <summary>The vertical collision radius of the front view, in pixels.</summary>
    internal const int ForwardFacingYRadius = 24;

    /// <summary>
    /// The pixels the forward-facing setup raises Samus when she turns to face the screen from a pose of another height.
    /// </summary>
    internal const int ForwardFacingSetupLift = 3;

    /// <summary>
    /// The whole pixels a changed pose must move Samus's centre by for its collision probe to test the intermediate
    /// block-step boundary before the full move.
    /// </summary>
    internal const int IntermediateProbeExpansion = 8;

    /// <summary>The pixels a jump started from a crouch raises Samus before the jump begins.</summary>
    internal const int CrouchJumpLift = 10;

    /// <summary>How far a pose change from a normal jump lifts Samus: one pixel up, taken off her position.</summary>
    internal static Offset16<RoomPixels> NormalJumpLift { get; } = new(0, 1);

}
