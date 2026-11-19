using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The state of a Norfair Rio parent; its flame follower keeps a <see cref="NorfairRioFollowerState"/>.</summary>
public sealed class NorfairRioEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NorfairRioEnemyState(NorfairRioAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<NorfairRioSegment> Playback { get; set; }

    /// <summary>
    /// Whether the Rio rests in its idle animation, during which its flames stay hidden (#2145, #2168): the idle animation
    /// is the one playing (#2450).
    /// </summary>
    internal bool Idles => Playback.PlayingSegment == NorfairRioSegment.Idle;

    /// <summary>
    /// Whether the Rio's swoop's start has finished, as its animation's cue says, and its main AI has not yet acted on it.
    /// It stays a mark beside the action rather than a phase of it (#2450): the ascent's start can finish on the update
    /// before the Rio lands on its perch, or after it has landed, while its landing's update and its wait for Samus run.
    /// The finish then carries into that wait, where it rests the Rio idle the next time it does not attack, or ends at
    /// once the takeoff of an attack it starts first.
    /// </summary>
    public RioAnimationFinish Finish { get; } = new();

    /// <summary>
    /// The flame follower's vertical placement relative to its parent, -16 through 12 pixels, negative above it. Private
    /// animation cues publish it with <see cref="Flames"/>.
    /// </summary>
    public int FollowerYOffset { get; internal set; }

    /// <summary>Which flames the follower shows, as the cue that placed them names (#2450).</summary>
    internal NorfairRioFlames Flames { get; set; }

    /// <summary>The Norfair Rio's velocity each update, positive right and down.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>The behaviour the parent runs.</summary>
    public NorfairRioAction Action { get; internal set; }
}
