namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Verification access to <see cref="SporeSpawnEnemyState"/> members production does not use.</summary>
internal static class SporeSpawnEnemyStateAccess
{
    /// <summary>Exposes <see cref="SporeSpawnEnemyState"/> state that only verification reads.</summary>
    extension(SporeSpawnEnemyState self)
    {
        /// <summary>The fade-target colours staged and not yet taken, in staging order.</summary>
        internal IReadOnlyList<SporeSpawnStagedColour> StagedTargetColours => self.stagedTargetColours;
    }
}
