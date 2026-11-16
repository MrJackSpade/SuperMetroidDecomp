using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The state of one Rio.</summary>
public sealed class RioEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal RioEnemyState(RioAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<RioSegment> Playback { get; set; }

    /// <summary>Signed 1/256-pixel horizontal dive velocity retained while the hovering phase temporarily stops X.</summary>
    public SignedEightEight SavedHorizontalVelocity { get; internal set; }

    /// <summary>The behaviour that runs after the main AI's RNG advance.</summary>
    public RioAction Action { get; internal set; }

    /// <summary>The Rio's velocity in 256ths of a pixel an update, positive right and down.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Handshake set by the animation-finished cue and consumed by main AI.</summary>
    public bool AnimationFinished { get; internal set; }
}
