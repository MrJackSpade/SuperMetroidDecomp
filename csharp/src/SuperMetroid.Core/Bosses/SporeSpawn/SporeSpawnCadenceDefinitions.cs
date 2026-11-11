namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The updates on which the dead Spore Spawn raises dust (#1606).</summary>
internal static class SporeSpawnCadenceDefinitions
{
    /// <summary>
    /// The updates on which the settled Spore Spawn raises a dust cloud at a random spot: every sixteenth update of the
    /// frame counter of 0 through 65535, on its first place. Each one draws a random number.
    /// </summary>
    internal static readonly UpdateCadence IdleDustEverySixteenthUpdate = new(16, 0);
}
