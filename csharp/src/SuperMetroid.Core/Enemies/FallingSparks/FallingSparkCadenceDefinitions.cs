namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>The updates on which a falling spark leaves its trail (#1606).</summary>
internal static class FallingSparkCadenceDefinitions
{
    /// <summary>
    /// The updates on which a falling spark spawns a trail spark behind it: those whose count falls on the period's first
    /// place.
    /// </summary>
    internal static readonly UpdateCadence SparkTrail = new(4, 0);
}
