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

    /// <summary>Set by a private animation cue.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// Cleared and set by its hide and show cues. The enemy listed after it reads this
    /// flag from its parent and shows its lower sprite only while it is set.
    /// </summary>
    public bool FollowerVisible { get; internal set; }

    /// <summary>Signed 1/256-pixel velocity in room pixels per update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The parent's current behaviour.</summary>
    public LowerNorfairRioAction Action { get; internal set; }
}
