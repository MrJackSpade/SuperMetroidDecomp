using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification seeding of <see cref="EnemyFrameContext"/> through its production operations.</summary>
internal static class EnemyFrameContextAccess
{
    /// <summary>Seeds the frame context's counters as a captured native state holds them.</summary>
    extension(EnemyFrameContext self)
    {
        /// <summary>Advances the random counter, as the end of each enemy pass does, until it holds <paramref name="value"/>.</summary>
        internal void SeedRandomEnemyCounter(int value)
        {
            Ensure.BetweenInclusive(0, ushort.MaxValue, value);
            while (self.RandomEnemyCounter != value)
                self.AdvanceRandomEnemyCounter();
        }

        /// <summary>
        /// Begins a frame, as the enemy pass does, with <paramref name="frameCounter"/>, Samus's
        /// <paramref name="projectiles"/> and her <paramref name="colours"/>, keeping the freeze flag.
        /// </summary>
        internal void SeedFrame(int frameCounter, SamusProjectileSystem projectiles, SamusColours colours) =>
            self.BeginFrame(self.TimeIsFrozen, projectiles, colours, frameCounter);
    }
}
