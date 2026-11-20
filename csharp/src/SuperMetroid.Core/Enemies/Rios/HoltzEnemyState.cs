using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of a Holtz parent; its flame follower keeps a <see cref="HoltzFollowerState"/>. This
/// variant has a parent and a follower as the Geruta does, but a visibility switch of its own.
/// </summary>
public sealed class HoltzEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal HoltzEnemyState(HoltzAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HoltzSegment> Playback { get; set; }

    /// <summary>
    /// The finish of its wind-up, its ascent's opening or its cooldown animation (#2562), from the animation's end
    /// until its next step acts on it (#2450). An animation that ends early carries into the next wait and ends it at
    /// once: the ascent's opening can finish on the update before the Rio lands, ending its wait for the cooldown
    /// animation at once; the cooldown animation, still playing then, can finish while the Rio waits for Samus, and an
    /// attack it starts on its next update then takes off at once.
    /// </summary>
    public RioAnimationFinish Finish { get; } = new();

    /// <summary>
    /// Whether its flames, the enemy listed after it, show; the hide and show cues clear and set it.
    /// </summary>
    public bool FollowerVisible { get; internal set; }

    /// <summary>Velocity in room pixels per update.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>The parent's current behaviour.</summary>
    public HoltzAction Action { get; internal set; }
}
