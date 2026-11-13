using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The state of one Norfair Rio, parent or flame follower.</summary>
public sealed class NorfairRioEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<NorfairRioProgram> Playback { get; set; }

    /// <summary>The last animation installed by function seven.</summary>
    internal NorfairRioProgram InstalledProgram { get; set; }

    /// <summary>Animation handshake set by the swoop-start-finished cue.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// The flame follower's placement relative to its parent; only Y is meaningful (-16 through 12 pixels). Private
    /// animation cues publish it, and its sign selects the follower's ascending or descending flames.
    /// </summary>
    public Offset16<RoomPixels> FollowerOffset { get; internal set; }

    /// <summary>Signed 8.8 velocity; the horizontal component feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The behaviour shared by parent and flame-follower dispatch.</summary>
    public NorfairRioEnemyFunction? Function { get; internal set; }

    /// <summary>Whether a negative population parameter 1 selects the flame follower instead of the parent; a follower must immediately follow a parent of this same enemy definition.</summary>
    public bool IsFollower { get; internal set; }
}
