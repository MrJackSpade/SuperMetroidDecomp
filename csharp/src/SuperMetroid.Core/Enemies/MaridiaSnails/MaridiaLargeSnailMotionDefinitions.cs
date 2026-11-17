namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>How the large Maridia snail pushes and shoves Samus (#2043), attacks, rolls and bounces (#2156).</summary>
internal static class MaridiaLargeSnailMotionDefinitions
{
    /// <summary>
    /// How far toward Samus the snail reaches while it pushes her, in pixels: its move tests the terrain that much
    /// further ahead, then draws back.
    /// </summary>
    internal const int PushReach = 16;

    /// <summary>How far the snail shoves Samus sideways when she touches it, in pixels.</summary>
    internal const int ShoveDistance = 4;

    /// <summary>How close horizontally Samus comes, in pixels and exclusive, for the snail to push or carry her.</summary>
    internal const int PushingReachX = 24;

    /// <summary>How close vertically Samus comes, in pixels and exclusive, to be in the snail's pushing band.</summary>
    internal const int PushingReachY = 32;

    /// <summary>How close horizontally Samus comes, in pixels and exclusive, for the snail to attack once its cooldown ends.</summary>
    internal const int AttackReach = 32;

    /// <summary>
    /// How many rolling updates a new or attacking snail waits before it may rotate into an attack, the one it first may on
    /// included (#2340).
    /// </summary>
    internal const int InitialAttackCooldown = 129;

    /// <summary>How many impacts the snail's landing or ledge fall settles in.</summary>
    internal const int SettlingBounces = 3;

    /// <summary>How far each update moves the snail's vertical place along the quadratic speed curve, a mark and a half.</summary>
    internal static QuadraticSpeedCurvePlace BounceAcceleration { get; } = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(384);

    /// <summary>The place along the quadratic speed curve the snail's fall stops accelerating at before its next impact.</summary>
    internal static QuadraticSpeedCurvePlace FastestFallPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(64);

    /// <summary>How far back along the quadratic speed curve each floor impact sets the snail.</summary>
    internal static QuadraticSpeedCurvePlace ImpactCurveLoss { get; } = QuadraticSpeedCurvePlace.FromMarks(16);

    /// <summary>The speed the snail rolls at, one pixel an update.</summary>
    internal static SignedSixteenSixteen RollSpeed { get; } = SignedSixteenSixteen.FromWholePixels(1);
}
