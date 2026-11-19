using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Per-pass enemy frame context: the random number and the frame counter, and the frozen-time, process-all and
/// deferred-camera flags for the current enemy pass. Samus, her projectiles, her colours and her bombs reach enemies
/// through each update's own context (#2159).
/// </summary>
[RoomLifetime]
internal sealed class EnemyFrameContext
{
    /// <summary>Binds the enemy pass to the session that holds the random enemy counter.</summary>
    /// <param name="session">The enemy state that outlives each room.</param>
    internal EnemyFrameContext(EnemySession session) => _session = Ensure.NotNull(session);

    /// <summary>The enemy state that outlives each room.</summary>
    private readonly EnemySession _session;

    /// <summary>
    /// The count of enemy passes, read by AI and projectiles: every reader takes a power-of-two cadence of it (an
    /// <see cref="UpdateCadence"/> or <see cref="UpdateCounterField"/>), so it is a plain count.
    /// </summary>
    internal int EnemyPassCount
    {
        get => _session.EnemyPassCount;
        private set => _session.EnemyPassCount = value;
    }

    /// <summary>
    /// The frame clock's count for the current enemy pass. It is independent of the enemies' own cycle of 256 updates.
    /// Readers take it modulo a power of two.
    /// </summary>
    internal int FrameCounter { get; private set; }

    /// <summary>Forces every enemy place through the current pass when processing requires a full sweep.</summary>
    internal bool ProcessAllEnemies { get; private set; }

    /// <summary>Freeze flag captured during the enemy pass and retained for the draw hooks that follow it.</summary>
    internal bool TimeIsFrozen { get; private set; }

    /// <summary>Defers init-AI camera reads until the door loader installs its final camera after the door-transition scroll.</summary>
    internal bool DeferLoaderTimeCameraReads { get; private set; }

    /// <summary>Room load: records whether init AI defers its camera reads.</summary>
    internal void BeginRoomLoad(bool deferLoaderTimeCameraReads) => DeferLoaderTimeCameraReads = deferLoaderTimeCameraReads;


    /// <summary>Forces every enemy place through the passes that follow, as Draygon and Spore Spawn request.</summary>
    internal void RequestFullSweep() => ProcessAllEnemies = true;

    /// <summary>Enemy frame start: the freeze flag and the frame clock's count.</summary>
    internal void BeginFrame(bool timeIsFrozen, int frameCounter)
    {
        TimeIsFrozen = timeIsFrozen;
        FrameCounter = frameCounter;
    }

    /// <summary>Counts the enemy pass that has just ended.</summary>
    internal void CountEnemyPass() => EnemyPassCount++;
}
