using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The state of a jumping Squeept, the parent its cosmetic follower (<see cref="NorfairLavaJumperFollowerState"/>) follows.</summary>
public sealed partial class SqueeptState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal SqueeptState(SqueeptAction action) => Action = action;

    /// <summary>Where the typed animation (#641) resumes; a start of the animation playing leaves it running (#2145).</summary>
    internal AnimationPlayback<NorfairLavaJumperSegment> Playback { get; set; }

    /// <summary>
    /// Signed vertical velocity in pixels per update. Gravity adds to it (<see cref="ApplyGravity"/>), the jump flips once
    /// the rise has slowed to the animation switch velocity, and the follower shows only while it is rising.
    /// </summary>
    public SignedEightEight YVelocity { get; internal set; }

    /// <summary>The jumper's population position (Y at the lava surface), where it returns after every jump.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>The jump's phase.</summary>
    public SqueeptAction Action { get; internal set; }

    /// <summary>
    /// One update's gravity: adds <paramref name="gravity"/> to the velocity. A jump launches at about eight pixels an update
    /// and falls back into the lava's row at about that speed, so the jumper never moves much faster than eight pixels an
    /// update either way.
    /// </summary>
    /// <param name="gravity">The gravity step, in pixels per update.</param>
    internal void ApplyGravity(SignedEightEight gravity) => YVelocity = YVelocity.Accelerated(gravity);
}
