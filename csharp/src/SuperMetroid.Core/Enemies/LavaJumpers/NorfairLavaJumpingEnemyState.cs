using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The state of a jumping Squeept, the parent its cosmetic follower (<see cref="NorfairLavaJumperFollowerState"/>) follows.</summary>
public sealed class NorfairLavaJumpingEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NorfairLavaJumpingEnemyState(NorfairLavaJumpingEnemyAction action) => Action = action;

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
    /// Signed vertical velocity in pixels per update. Gravity adds to it (<see cref="ApplyGravity"/>), the jump flips once
    /// the rise has slowed to the animation switch velocity, and the follower shows only while it is rising.
    /// </summary>
    public SignedEightEight YVelocity { get; internal set; }

    /// <summary>The jumper's population position (Y at the lava surface), where it returns after every jump.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>The jump's phase.</summary>
    public NorfairLavaJumpingEnemyAction Action { get; internal set; }

    /// <summary>
    /// One update's gravity: adds <paramref name="gravity"/> to the velocity. A jump launches at about eight pixels an update
    /// and falls back into the lava's row at about that speed, so the velocity stays far inside its type; a velocity
    /// past the type's range fails loudly.
    /// </summary>
    /// <param name="gravity">The 1/256-pixel gravity step.</param>
    internal void ApplyGravity(SignedEightEight gravity) => YVelocity = YVelocity.Accelerated(gravity);
}
