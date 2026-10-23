using SuperMetroid.Core.Samus.Palettes;
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

        /// <summary>Begins a frame with <paramref name="nmiFrameCounter"/>, keeping the freeze flag and projectiles.</summary>
        internal void SeedNmiFrameCounter(int nmiFrameCounter) =>
            self.BeginFrame(self.TimeIsFrozen, self.SamusProjectiles, self.SamusColours, nmiFrameCounter);

        /// <summary>Begins a frame with <paramref name="projectiles"/>, keeping the freeze flag and frame counter.</summary>
        internal void SeedSamusProjectiles(SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem? projectiles) =>
            self.BeginFrame(self.TimeIsFrozen, projectiles, self.SamusColours, self.NmiFrameCounter);
    }
}
