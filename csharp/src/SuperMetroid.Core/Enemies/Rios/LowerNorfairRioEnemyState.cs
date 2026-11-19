using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Lower Norfair Rio parent; its flame follower keeps a <see cref="LowerNorfairRioFollowerState"/>. This
/// variant has a parent and a follower as the Norfair Rio does, but a visibility switch of its own.
/// </summary>
public sealed class LowerNorfairRioEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal LowerNorfairRioEnemyState(LowerNorfairRioAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<LowerNorfairRioSegment> Playback { get; set; }

    /// <summary>
    /// The finish of its wind-up, its ascent's opening or its cooldown animation, a typed carried event of its action (#2562): raised by the animation's finishing
    /// cue and cleared as its main AI moves on. It stays a mark beside the action rather than a phase of it (#2450): the
    /// ascent's opening can finish on the update before the Rio lands, which carries the finish into its wait for the
    /// cooldown animation and ends that wait at once; the cooldown animation, still playing then, can finish while the Rio
    /// waits for Samus, and an attack it starts on its next update then takes off at once.
    /// </summary>
    public RioAnimationFinish Finish { get; } = new();

    /// <summary>
    /// Whether the enemy listed after it, its follower, shows its lower sprite; the hide
    /// and show cues clear and set it.
    /// </summary>
    public bool FollowerVisible { get; internal set; }

    /// <summary>Velocity in room pixels per update.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>The parent's current behaviour.</summary>
    public LowerNorfairRioAction Action { get; internal set; }
}
