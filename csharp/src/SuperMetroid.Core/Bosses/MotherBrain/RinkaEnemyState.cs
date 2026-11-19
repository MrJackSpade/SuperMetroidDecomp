using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of a Rinka.</summary>
public sealed class RinkaEnemyState
{
    /// <summary>The typed animation (#641, #1600) and where it stands.</summary>
    internal AnimationPlayback<RinkaSegment> Playback { get; set; }

    /// <summary>
    /// Starts the state in <paramref name="action"/>, playing the animation starting at <paramref name="animation"/>
    /// (#2319), with the shared <paramref name="spawnPoint"/> a special Rinka starts at.
    /// </summary>
    internal RinkaEnemyState(RinkaAction action, RinkaSegment animation, RinkaSpawnPoint? spawnPoint)
    {
        Action = action;
        Playback = Starting(animation);
        SpawnPoint = spawnPoint;
    }

    /// <summary>The Rinka's animation starting at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<RinkaSegment> Starting(RinkaSegment segment) =>
        new(RinkaAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// The behaviour the Rinka runs each update, changed by initialization, the fire step, aim completion, or its own
    /// death.
    /// </summary>
    public RinkaAction Action { get; internal set; }

    /// <summary>Velocity, aimed at where Samus stood as the aim delay ran out.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The spawn point a special Mother Brain Rinka holds, which no other Rinka can take until it is released; null
    /// when it holds none.
    /// </summary>
    public RinkaSpawnPoint? HeldSpawnPoint { get; internal set; }

    /// <summary>
    /// The spawn point a special Mother Brain Rinka appears and respawns at (#2256): the one its spawn settings start it
    /// at, then whichever it last reserved, kept while every point is held. Null for an ordinary Rinka, which returns to
    /// its own placement.
    /// </summary>
    public RinkaSpawnPoint? SpawnPoint { get; internal set; }

    /// <summary>
    /// The wait before the Rinka aims or, for a special Rinka, before it respawns: one of its two waits at a time (#2253).
    /// </summary>
    public BossCountdown DelayTimer { get; } = new();
}
