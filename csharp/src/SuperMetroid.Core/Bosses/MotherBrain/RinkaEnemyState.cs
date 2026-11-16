using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of a Rinka.</summary>
public sealed class RinkaEnemyState
{
    /// <summary>The typed animation (#641, #1600) and where it stands.</summary>
    internal AnimationPlayback<RinkaSegment> Playback { get; set; } = new(RinkaAnimations.StartingWith(default), AnimationPlace.StartOfAnimation);

    /// <summary>Starts the state in <paramref name="action"/>, with the shared <paramref name="spawnPoint"/> a special Rinka starts at.</summary>
    internal RinkaEnemyState(RinkaAction action, RinkaSpawnPoint? spawnPoint)
    {
        Action = action;
        SpawnPoint = spawnPoint;
    }

    /// <summary>
    /// The behaviour the Rinka runs each update, changed by initialization, the fire step, aim completion, or the special
    /// death tail.
    /// </summary>
    public RinkaAction Action { get; internal set; }

    /// <summary>Signed 1/256-pixel velocity: X produced by the sine multiply, Y by the cosine multiply.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

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

    /// <summary>Updates left before flight or before a special respawn, counted down past zero.</summary>
    public int DelayTimer { get; internal set; }
}
