namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>How high a Beetom's leaps and lunge climb, and how fast each climbs along the quadratic speed curve (#2026).</summary>
internal static class BeetomLeapDefinitions
{
    /// <summary>The short leap: it climbs 48 pixels, four marks a pass.</summary>
    internal static QuadraticCurveClimb ShortLeap { get; } = new(SignedSixteenSixteen.FromWholePixels(48), QuadraticSpeedCurvePlace.FromMarks(4));

    /// <summary>The long leap: it climbs 64 pixels, five marks a pass.</summary>
    internal static QuadraticCurveClimb LongLeap { get; } = new(SignedSixteenSixteen.FromWholePixels(64), QuadraticSpeedCurvePlace.FromMarks(5));

    /// <summary>The lunge at Samus: it climbs 48 pixels, three marks a pass.</summary>
    internal static QuadraticCurveClimb Lunge { get; } = new(SignedSixteenSixteen.FromWholePixels(48), QuadraticSpeedCurvePlace.FromMarks(3));
}
