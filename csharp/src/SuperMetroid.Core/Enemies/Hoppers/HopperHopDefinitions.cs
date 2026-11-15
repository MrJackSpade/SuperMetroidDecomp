namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// How high a hopper's two hops climb and how fast each climbs along the quadratic speed curve (#2026). Every hopper
/// definition authors the same two hops.
/// </summary>
internal static class HopperHopDefinitions
{
    /// <summary>The small hop: it climbs 16 pixels, three marks a pass.</summary>
    internal static QuadraticCurveClimb SmallHop { get; } = new(SignedSixteenSixteen.FromWholePixels(16), 3);

    /// <summary>The big hop: it climbs 48 pixels, four marks a pass.</summary>
    internal static QuadraticCurveClimb BigHop { get; } = new(SignedSixteenSixteen.FromWholePixels(48), 4);
}
