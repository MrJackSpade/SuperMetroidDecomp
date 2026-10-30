namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What the suit pickup's light beam leaves behind for a later shinespark windup to read as the fraction of its
/// vertical acceleration (#1468). The pickup records its progress on every stage call: the lines the beam has grown or
/// been wiped by while it grows, reveals the suit or shrinks, and both of its edge columns, the right one scaled by a
/// screen line's width, while it is a column or the centre line. A windup that starts before anything else replaces
/// that value accelerates by it, so the value is gameplay-observable; how the light is drawn is not.
/// </summary>
/// <remarks>
/// Consulted when the suit pickup publishes its stage, at setup and after every stage call. The original game kept the
/// light's progress in a working word the shinespark windup later read as its acceleration; the suit pickup
/// shared-scratch check confirms the exact value at every stage of a Varia transformation.
/// </remarks>
[CompatibilityService("a shinespark windup after a suit pickup accelerates by the light beam's last recorded progress")]
[EncodingOwner("suit pickup light beam progress the shinespark reads")]
public static class SuitPickupSparkCarryover
{
    /// <summary>The value left while the beam is measured in lines grown or wiped, 0 through 255.</summary>
    public static int OfLines(int lines) => Ensure.BetweenInclusive(0, byte.MaxValue, lines);

    /// <summary>The value left while the beam is a column between two edge columns, each 0 through 255.</summary>
    public static int OfEdges(int left, int right) =>
        Ensure.BetweenInclusive(0, byte.MaxValue, left) +
        Ensure.BetweenInclusive(0, byte.MaxValue, right) * ScreenGeometry.ScreenWidthPixels;
}
