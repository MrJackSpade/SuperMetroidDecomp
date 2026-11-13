using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of a Rinka.</summary>
public sealed class RinkaEnemyState
{
    /// <summary>The typed animation (#641, #1600) and where it stands.</summary>
    internal AnimationPlayback<RinkaSegment> Playback { get; set; } = new(RinkaAnimations.StartingWith(default), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// Behaviour dispatched by main AI, changed by initialization, the fire step, aim completion, or the special death
    /// tail.
    /// </summary>
    public RinkaAction? Action { get; internal set; }

    /// <summary>Signed 8.8 velocity: X produced by the sine multiply, Y by the cosine multiply.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Reservation token of the spawn location a special Mother Brain Rinka holds: one of the even tokens 2 through 22
    /// from the spawn-point table, or zero when it holds none.
    /// </summary>
    public int SpawnResourceToken { get; internal set; }

    /// <summary>Updates left before flight or before a special respawn, counted down past zero.</summary>
    public int DelayTimer { get; internal set; }
}
