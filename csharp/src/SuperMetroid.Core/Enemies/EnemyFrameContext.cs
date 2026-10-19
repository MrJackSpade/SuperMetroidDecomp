using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Per-pass enemy frame context: the random and NMI frame counters, the live Samus projectile system,
/// and the frozen-time, process-all and deferred-camera flags for the current enemy pass.
/// </summary>
internal sealed class EnemyFrameContext
{
    /// <summary>
    /// Enemy-pass random counter read by native AI and projectile routines, 0 through 65535. Its one advance wraps
    /// from 65535 back to zero, and every reader takes its low bits or copies it whole, so a wider store holds exactly
    /// the counter the cartridge keeps.
    /// </summary>
    internal int _randomEnemyCounter;

    /// <summary>
    /// The sixteen-bit frame counter for the current enemy pass, 0 through 65535, copied from the frame clock (or the
    /// random counter when the pass has none). It is independent of the eight-bit counter; standalone audits seed both
    /// from the same enemy clock. Readers take its low bits.
    /// </summary>
    internal int _enemyFrameNmiFrameCounter;
    /// <summary>Live projectile system supplied by EnemyMain to both ordinary and custom enemy-touch callbacks.</summary>
    internal SamusProjectileSystem? _samusProjectilesForEnemyFrame;
    /// <summary>Forces every enemy slot through the current pass when native processing requires a full sweep.</summary>
    internal bool _processAllEnemies;

    /// <summary>Freeze flag captured during the enemy pass and retained for the draw hooks that follow it.</summary>
    internal bool _enemyFrameTimeIsFrozen;

    /// <summary>Defers init-AI layer-1 camera reads until the door loader installs its final camera after IRQ scrolling.</summary>
    internal bool _deferLoaderTimeCameraReads;
}
