using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The state of the Morph Ball eye's body. Its beam mount shares the definition and keeps a
/// <see cref="MorphBallEyeMountState"/> of its own (#2164).
/// </summary>
public sealed partial class MorphBallEyeEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal MorphBallEyeEnemyState(MorphBallEyeAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600): the eyelid's.</summary>
    internal AnimationPlayback<MorphBallEyeSegment> Playback { get; set; }

    /// <summary>
    /// The pose the open eye looks along while it tracks Samus (#2070), chosen from its angle on each update it tracks; null
    /// while its eyelid animation shows instead.
    /// </summary>
    internal EnemyFrameId? Aim { get; set; }

    /// <summary>
    /// Whether the eye is activated: set once its beam appears, cleared once
    /// the body starts closing. The beam keeps cycling its colours only while it is set.
    /// </summary>
    public bool IsActivated { get; internal set; }

    /// <summary>
    /// The aim angle, zero up and turning clockwise.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>
    /// The updates left of the eye's opening or closing, the one it ends on included: each starts at
    /// <see cref="MorphBallEyeWatchDefinitions.TransitionDuration"/>, and the update that takes it to zero ends it (#2340).
    /// </summary>
    public UpdateCountdown ActionTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The action the eye performs.</summary>
    public MorphBallEyeAction Action { get; internal set; }
}
