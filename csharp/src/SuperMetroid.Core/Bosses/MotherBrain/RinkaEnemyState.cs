using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of a Rinka.</summary>
public sealed class RinkaEnemyState
{
    /// <summary>The typed animation (#641, #1600) and where it stands.</summary>
    internal AnimationPlayback<RinkaSegment> Playback { get; set; }

    /// <summary>
    /// Starts the state in <paramref name="action"/>, playing the animation starting at <paramref name="animation"/>
    /// (#2319), spawning as <paramref name="spawning"/> says: a special Rinka shares Mother Brain's room's spawn points from
    /// the one it starts at.
    /// </summary>
    internal RinkaEnemyState(RinkaAction action, RinkaSegment animation, RinkaSpawnSettings spawning)
    {
        Action = action;
        Playback = Starting(animation);
        SharedSpawn = Ensure.NotNull(spawning) is RinkaSpawnSettings.SharingPoints sharing ? new RinkaSharedSpawn(sharing.Start) : null;
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
    /// How a special Mother Brain Rinka shares the room's spawn points (#2256): the one it appears and respawns at, and the
    /// one it holds. An ordinary Rinka, which returns to its own placement, shares none.
    /// </summary>
    public RinkaSharedSpawn? SharedSpawn { get; }

    /// <summary>The wait before the Rinka aims at Samus, which each life starts (#2253, #2572).</summary>
    public BossCountdown AimDelay { get; } = new();

    /// <summary>The wait before a special Rinka killed by Samus respawns, which its death starts (#2253, #2572).</summary>
    public BossCountdown RespawnDelay { get; } = new();
}
