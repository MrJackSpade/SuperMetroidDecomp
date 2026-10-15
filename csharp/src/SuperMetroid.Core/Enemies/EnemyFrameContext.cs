using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Per-pass enemy frame context: the random and NMI frame counters, the live Samus projectile system,
/// and the frozen-time, process-all and deferred-camera flags for the current enemy pass.
/// </summary>
internal sealed class EnemyFrameContext
{
    /// <summary>Enemy-pass random counter read by native AI and projectile routines.</summary>
    internal ushort _randomEnemyCounter;

    /// <summary>
    /// The 16-bit NMI_FrameCounter ($05B6) for the current enemy pass. It is independent of
    /// the 8-bit $05B5 counter; standalone audits seed both from the same enemy clock.
    /// </summary>
    internal ushort _enemyFrameNmiFrameCounter;
    /// <summary>Live projectile system supplied by EnemyMain to both ordinary and custom enemy-touch callbacks.</summary>
    internal SamusProjectileSystem? _samusProjectilesForEnemyFrame;
    /// <summary>Forces every enemy slot through the current pass when native processing requires a full sweep.</summary>
    internal bool _processAllEnemies;

    /// <summary>Freeze flag captured during the enemy pass and retained for the draw hooks that follow it.</summary>
    internal bool _enemyFrameTimeIsFrozen;

    /// <summary>Defers init-AI layer-1 camera reads until the door loader installs its final camera after IRQ scrolling.</summary>
    internal bool _deferLoaderTimeCameraReads;
}
