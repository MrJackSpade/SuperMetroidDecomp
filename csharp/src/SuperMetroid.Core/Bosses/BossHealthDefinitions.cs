namespace SuperMetroid.Core.Bosses;

/// <summary>The health of the bosses Samus cannot wear down (#2171).</summary>
internal static class BossHealthDefinitions
{
    /// <summary>
    /// The full health the Tourian Shitroid and Ceres Ridley are refilled to on every update of their main behaviour, so
    /// no hits wear them down, kept exact (#1627): a hit later in the update takes its damage from this count, and what
    /// remains is the health the rest of that update reads.
    /// </summary>
    internal const int RefilledFullHealth = 32767;
}
