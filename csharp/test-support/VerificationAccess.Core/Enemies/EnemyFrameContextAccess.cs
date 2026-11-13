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
        /// <paramref name="projectiles"/> and her <paramref name="colours"/> and no power bomb exploding, keeping the freeze flag.
        /// </summary>
        internal void SeedFrame(int frameCounter, SamusProjectileSystem projectiles, SamusColours colours) =>
            self.BeginFrame(self.TimeIsFrozen, projectiles, colours, new SamusPowerBombExplosionState(), frameCounter);

        /// <summary>
        /// Begins a frame, as the enemy pass does, with <paramref name="frameCounter"/>, Samus's
        /// <paramref name="projectiles"/> and her <paramref name="colours"/> and the power bomb <paramref name="explosion"/>,
        /// keeping the freeze flag.
        /// </summary>
        internal void SeedFrame(int frameCounter, SamusProjectileSystem projectiles, SamusColours colours,
            SamusPowerBombExplosionState explosion) =>
            self.BeginFrame(self.TimeIsFrozen, projectiles, colours, explosion, frameCounter);

        /// <summary>
        /// Begins the next frame of an enemy pass already begun, as the enemy pass does, with the power bomb
        /// <paramref name="explosion"/> and everything else the begun frame holds.
        /// </summary>
        /// <exception cref="InvalidOperationException">No enemy frame has begun.</exception>
        internal void SeedPowerBombExplosion(SamusPowerBombExplosionState explosion) =>
            self.BeginFrame(self.TimeIsFrozen,
                self.SamusProjectiles ?? throw new InvalidOperationException("Seeding a power bomb explosion requires a begun enemy frame."),
                self.SamusColours ?? throw new InvalidOperationException("Seeding a power bomb explosion requires a begun enemy frame."),
                explosion, self.FrameCounter);
    }
}
