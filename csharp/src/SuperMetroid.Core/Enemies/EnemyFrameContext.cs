using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Per-pass enemy frame context: the random number and the frame counter, the live Samus projectile system,
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
    /// Enemy-pass random counter read by native AI and projectile routines, 0 through 65535. Its one advance wraps
    /// from 65535 back to zero, and every reader takes its low bits or copies it whole, so a wider store holds exactly
    /// the counter the cartridge keeps.
    /// </summary>
    internal int RandomEnemyCounter
    {
        get => _session.RandomEnemyCounter;
        private set => _session.RandomEnemyCounter = value;
    }

    /// <summary>
    /// The sixteen-bit frame counter for the current enemy pass, 0 through 65535, copied from the frame clock (or the
    /// random counter when the pass has none). It is independent of the eight-bit counter; standalone audits seed both
    /// from the same enemy clock. Readers take its low bits.
    /// </summary>
    internal int FrameCounter { get; private set; }
    /// <summary>Live projectile system supplied by EnemyMain to both ordinary and custom enemy-touch callbacks.</summary>
    internal SamusProjectileSystem? SamusProjectiles { get; private set; }
    /// <summary>Samus's colours for the pass, which Pseudo Screw's enemy contact changes (#1405).</summary>
    internal SamusColours? SamusColours { get; private set; }
    /// <summary>Forces every enemy slot through the current pass when native processing requires a full sweep.</summary>
    internal bool ProcessAllEnemies { get; private set; }

    /// <summary>Freeze flag captured during the enemy pass and retained for the draw hooks that follow it.</summary>
    internal bool TimeIsFrozen { get; private set; }

    /// <summary>Defers init-AI layer-1 camera reads until the door loader installs its final camera after the door-transition scroll.</summary>
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
    /// Enemy frame start: the freeze flag, the live projectiles, and the sixteen-bit frame counter, which standalone
    /// passes without a runtime clock take from the random counter.
    /// </summary>
    internal void BeginFrame(bool timeIsFrozen, SamusProjectileSystem? samusProjectiles, SamusColours? samusColours, int? frameCounter)
    {
        TimeIsFrozen = timeIsFrozen;
        SamusProjectiles = samusProjectiles;
        SamusColours = samusColours;
        FrameCounter = frameCounter ?? RandomEnemyCounter;
    }

    /// <summary>The end-of-pass advance of the random counter, wrapping from 65535 to zero.</summary>
    internal void AdvanceRandomEnemyCounter() => RandomEnemyCounter = WrappingCounter.IncrementWord(RandomEnemyCounter);
}
