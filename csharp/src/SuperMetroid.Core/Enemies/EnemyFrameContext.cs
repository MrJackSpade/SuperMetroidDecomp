using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Per-pass enemy frame context: the random number and the frame counter, the live Samus projectile system, Samus's
/// colours and the power bomb explosion,
/// and the frozen-time, process-all and deferred-camera flags for the current enemy pass.
/// </summary>
[RoomLifetime]
internal sealed class EnemyFrameContext
{
    /// <summary>Binds the enemy pass to the session that holds the random enemy counter.</summary>
    /// <param name="session">The enemy words that outlive each room.</param>
    internal EnemyFrameContext(EnemySession session) => _session = Ensure.NotNull(session);

    /// <summary>The enemy words that outlive each room.</summary>
    private readonly EnemySession _session;

    /// <summary>
    /// Enemy-pass random counter read by AI and projectile routines, 0 through 65535. Its one advance wraps
    /// from 65535 back to zero, and every reader takes it modulo a power of two or copies it whole, so a wider store holds exactly
    /// the counter the game keeps.
    /// </summary>
    internal int RandomEnemyCounter
    {
        get => _session.RandomEnemyCounter;
        private set => _session.RandomEnemyCounter = value;
    }

    /// <summary>
    /// The frame clock's count for the current enemy pass. It is independent of the enemies' own cycle of 256 updates.
    /// Readers take it modulo a power of two.
    /// </summary>
    internal int FrameCounter { get; private set; }
    /// <summary>Live projectile system supplied by the enemy pass to both ordinary and custom enemy-touch callbacks.</summary>
    internal SamusProjectileSystem? SamusProjectiles { get; private set; }
    /// <summary>Samus's colours for the pass, which Pseudo Screw's enemy contact changes (#1405).</summary>
    internal SamusColours? SamusColours { get; private set; }

    /// <summary>
    /// The power bomb explosion as the latest enemy pass saw it, which Norfair Ridley's dodge and Draygon's goop and turret
    /// shots read; the enemy projectile pass that follows the enemy pass reads the same one. Null before the first pass.
    /// </summary>
    internal SamusPowerBombExplosionState? PowerBombExplosion { get; private set; }
    /// <summary>Forces every enemy slot through the current pass when processing requires a full sweep.</summary>
    internal bool ProcessAllEnemies { get; private set; }

    /// <summary>Freeze flag captured during the enemy pass and retained for the draw hooks that follow it.</summary>
    internal bool TimeIsFrozen { get; private set; }

    /// <summary>Defers init-AI camera reads until the door loader installs its final camera after the door-transition scroll.</summary>
    internal bool DeferLoaderTimeCameraReads { get; private set; }

    /// <summary>Room load: records whether init AI defers its camera reads, and drops the previous room's projectiles.</summary>
    internal void BeginRoomLoad(bool deferLoaderTimeCameraReads)
    {
        DeferLoaderTimeCameraReads = deferLoaderTimeCameraReads;
        SamusProjectiles = null;
        SamusColours = null;
    }


    /// <summary>Forces every enemy slot through the passes that follow, as Draygon and Spore Spawn request.</summary>
    internal void RequestFullSweep() => ProcessAllEnemies = true;

    /// <summary>
    /// Enemy frame start: the freeze flag, the live projectiles, Samus's colours, the power bomb explosion, and the frame
    /// clock's count.
    /// </summary>
    internal void BeginFrame(bool timeIsFrozen, SamusProjectileSystem samusProjectiles, SamusColours samusColours,
        SamusPowerBombExplosionState powerBombExplosion, int frameCounter)
    {
        TimeIsFrozen = timeIsFrozen;
        SamusProjectiles = samusProjectiles;
        SamusColours = samusColours;
        PowerBombExplosion = Ensure.NotNull(powerBombExplosion);
        FrameCounter = frameCounter;
    }

    /// <summary>The end-of-pass advance of the random counter, wrapping from 65535 to zero.</summary>
    internal void AdvanceRandomEnemyCounter() => RandomEnemyCounter = WrappingCounter.IncrementCount(RandomEnemyCounter);
}
