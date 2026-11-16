using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The state of a Norfair Rio parent; its flame follower keeps a <see cref="NorfairRioFollowerState"/>.</summary>
public sealed class NorfairRioEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NorfairRioEnemyState(NorfairRioAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<NorfairRioSegment> Playback { get; set; }

    /// <summary>Whether the Rio rests in its idle animation, during which its flames stay hidden (#2145).</summary>
    internal bool Idles => ReferenceEquals(Playback.Animation, NorfairRioAnimations.StartingWith(NorfairRioSegment.Idle));

    /// <summary>Animation handshake set by the swoop-start-finished cue.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// The flame follower's vertical placement relative to its parent, -16 through 12 pixels. Private animation cues
    /// publish it, and its sign selects the follower's ascending or descending flames.
    /// </summary>
    public int FollowerYOffset { get; internal set; }

    /// <summary>The Norfair Rio's velocity in 256ths of a pixel an update, positive right and down.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The behaviour the parent runs.</summary>
    public NorfairRioAction Action { get; internal set; }
}
