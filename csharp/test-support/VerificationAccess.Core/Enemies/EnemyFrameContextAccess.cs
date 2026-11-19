namespace SuperMetroid.Core.Enemies;

/// <summary>Verification seeding of <see cref="EnemyFrameContext"/> through its production operations.</summary>
internal static class EnemyFrameContextAccess
{
    /// <summary>Seeds the frame context's counters as a captured native state holds them.</summary>
    extension(EnemyFrameContext self)
    {
        /// <summary>Counts enemy passes, as the end of each enemy pass does, until the count is <paramref name="value"/>.</summary>
        /// <exception cref="InvalidOperationException">The count has already passed <paramref name="value"/>.</exception>
        internal void SeedEnemyPassCount(int value)
        {
            Ensure.BetweenInclusive(0, ushort.MaxValue, value);
            if (self.EnemyPassCount > value)
                throw new InvalidOperationException($"The enemy pass count {self.EnemyPassCount} has already passed {value}.");
            while (self.EnemyPassCount != value)
                self.CountEnemyPass();
        }

        /// <summary>Begins a frame, as the enemy pass does, with <paramref name="frameCounter"/>, keeping the freeze flag.</summary>
        internal void SeedFrame(int frameCounter) => self.BeginFrame(self.TimeIsFrozen, frameCounter);
    }
}
