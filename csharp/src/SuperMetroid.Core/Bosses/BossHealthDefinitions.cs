namespace SuperMetroid.Core.Bosses;

/// <summary>The health of the bosses Samus cannot wear down (#2171).</summary>
internal static class BossHealthDefinitions
{
    /// <summary>
    /// The full health Ceres Ridley is refilled to on every update of his main behaviour, so no hits wear him down, kept
    /// exact (#1627): a hit later in the update takes its damage from this count, and what remains is the health the rest
    /// of that update reads.
    /// </summary>
    internal const int RefilledFullHealth = 32767;
}
