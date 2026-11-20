namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>When a Choot wakes and how its jump climbs the speed curve (#2279).</summary>
internal static class ChootMotionDefinitions
{
    /// <summary>The horizontal distance, in pixels, Samus must come strictly within for a waiting Choot to arm its jump.</summary>
    internal const int ActivationDistance = 80;

    /// <summary>
    /// How far the ascent moves back along the quadratic speed curve each jumping update, in whole marks; the starting
    /// place is found by moving on by the same step.
    /// </summary>
    internal static readonly QuadraticSpeedCurvePlace AscentStep = QuadraticSpeedCurvePlace.FromMarks(2);
}
