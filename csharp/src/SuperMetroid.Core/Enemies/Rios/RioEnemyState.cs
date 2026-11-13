using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The state of one Rio.</summary>
public sealed class RioEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<RioSegment> Playback { get; set; }

    /// <summary>Signed 8.8 horizontal dive velocity retained while the hovering phase temporarily stops X.</summary>
    public SignedEightEight SavedHorizontalVelocity { get; internal set; }

    /// <summary>The behaviour dispatched after the main AI's RNG advance.</summary>
    public RioAction? Action { get; internal set; }

    /// <summary>Signed 8.8 velocity; the horizontal component feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Handshake set by the animation-finished cue and consumed by main AI.</summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>The last animation installed by a behavioural transition; none at room load.</summary>
    internal RioSegment? InstalledAnimation { get; set; }
}
