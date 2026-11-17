namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>The updates on which a falling spark leaves its trail (#1606).</summary>
internal static class FallingSparkCadenceDefinitions
{
    /// <summary>
    /// The updates on which a falling spark spawns a trail spark behind it: every fourth update, those whose count
    /// divides evenly by four.
    /// </summary>
    internal static readonly UpdateCadence TrailEveryFourthUpdate = new(4, 0);
}
