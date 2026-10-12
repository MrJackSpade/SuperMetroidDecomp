using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Per-pass enemy frame context: the random and NMI frame counters, the live Samus projectile system,
/// and the frozen-time, process-all and deferred-camera flags for the current enemy pass.
/// </summary>
internal sealed class EnemyFrameContext
{
    internal ushort _randomEnemyCounter;

    /// <summary>
    /// The 16-bit NMI_FrameCounter ($05B6) for the current enemy pass. It is independent of
    /// the 8-bit $05B5 counter; standalone audits seed both from the same enemy clock.
    /// </summary>
    internal ushort _enemyFrameNmiFrameCounter;
    // Live dependency supplied by EnemyMain, shared by ordinary and custom touch callbacks.
    internal SamusProjectileSystem? _samusProjectilesForEnemyFrame;
    internal bool _processAllEnemies;

    // A door load initializes enemies while the door IRQ scrolls; init AIs that read
    // layer 1 then wait for the loader's camera (CompleteLoaderTimeCameraReads).
    // TimeIsFrozenFlag as the enemy frame saw it; the draw hooks that follow read it.
    internal bool _enemyFrameTimeIsFrozen;

    internal bool _deferLoaderTimeCameraReads;
}
