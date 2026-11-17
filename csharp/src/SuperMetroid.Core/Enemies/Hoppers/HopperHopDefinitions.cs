namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// How high a hopper's two hops climb and how fast each climbs along the quadratic speed curve (#2026). Every hopper
/// definition authors the same two hops.
/// </summary>
internal static class HopperHopDefinitions
{
    /// <summary>The small hop: it climbs 16 pixels, three marks a pass.</summary>
    internal static QuadraticCurveClimb SmallHop { get; } = new(SignedSixteenSixteen.FromWholePixels(16), QuadraticSpeedCurvePlace.FromMarks(3));

    /// <summary>The big hop: it climbs 48 pixels, four marks a pass.</summary>
    internal static QuadraticCurveClimb BigHop { get; } = new(SignedSixteenSixteen.FromWholePixels(48), QuadraticSpeedCurvePlace.FromMarks(4));

    /// <summary>How fast either hop carries a hopper sideways, in whole pixels an update.</summary>
    internal const int HorizontalSpeed = 3;

    /// <summary>The place along the quadratic speed curve, mark 64, that caps a hop's return to its surface (#2339).</summary>
    internal static QuadraticSpeedCurvePlace FastestReturnPlace { get; } = QuadraticSpeedCurvePlace.FromMarks(64);
}
