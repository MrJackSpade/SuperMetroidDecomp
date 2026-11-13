using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The state of one Squeept part, the jumping parent or its cosmetic follower.</summary>
public sealed class NorfairLavaJumpingEnemyState
{
    /// <summary>
    /// Whether the jump animation has finished. The flip animation's cue sets this handshake after the
    /// jump animation reaches the frame where main AI may begin the final falling phase.
    /// </summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>
    /// The animation start remembers the last started animation so it does not restart one
    /// which is already running; none since initialization.
    /// </summary>
    internal NorfairLavaJumperSegment? InstalledAnimation { get; set; }

    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationPlayback<NorfairLavaJumperSegment> Playback { get; set; }

    /// <summary>
    /// Signed 8.8 vertical velocity. Gravity adds to it, and the jump compares it unsigned with the animation
    /// switch velocity (<see cref="ApplyGravity"/>), and the follower shows only while it is negative.
    /// </summary>
    public SignedEightEight YVelocity { get; internal set; }

    /// <summary>Parent variant's immutable population position (Y at the lava surface).</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>Parent parts traverse the four jump phases, while followers keep the follow action.</summary>
    public NorfairLavaJumpingEnemyAction? Action { get; internal set; }

    /// <summary>Whether this is the visual follower half rather than the jumping parent.</summary>
    public bool IsFollower { get; internal set; }

    /// <summary>One update's gravity: adds <paramref name="gravity"/> to the velocity, wrapping as the game's addition does.</summary>
    /// <param name="gravity">The 8.8 gravity step.</param>
    internal void ApplyGravity(SignedEightEight gravity) => YVelocity += gravity;
}
