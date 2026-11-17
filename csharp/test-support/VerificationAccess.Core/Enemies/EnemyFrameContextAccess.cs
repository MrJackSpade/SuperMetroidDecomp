namespace SuperMetroid.Core.Enemies;

/// <summary>Verification seeding of <see cref="EnemyFrameContext"/> through its production operations.</summary>
internal static class EnemyFrameContextAccess
{
    /// <summary>Seeds the frame context's counters as a captured native state holds them.</summary>
    extension(EnemyFrameContext self)
    {
        /// <summary>Advances the random counter, as the end of each enemy pass does, until it holds <paramref name="value"/>.</summary>
        /// <exception cref="InvalidOperationException">The counter has already passed <paramref name="value"/>.</exception>
        internal void SeedRandomEnemyCounter(int value)
        {
            Ensure.BetweenInclusive(0, ushort.MaxValue, value);
            if (self.RandomEnemyCounter > value)
                throw new InvalidOperationException($"The enemy pass count {self.RandomEnemyCounter} has already passed {value}.");
            while (self.RandomEnemyCounter != value)
                self.AdvanceRandomEnemyCounter();
        }

        /// <summary>Begins a frame, as the enemy pass does, with <paramref name="frameCounter"/>, keeping the freeze flag.</summary>
        internal void SeedFrame(int frameCounter) => self.BeginFrame(self.TimeIsFrozen, frameCounter);
    }
}
