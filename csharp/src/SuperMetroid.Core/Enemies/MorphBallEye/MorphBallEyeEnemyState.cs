using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The state of the Morph Ball eye. The mount and eye body share a definition, but their spawn part selects their
/// role.
/// </summary>
public sealed class MorphBallEyeEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal MorphBallEyeEnemyState(MorphBallEyeAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600): the eyelid's or the mount's.</summary>
    internal AnimationPlayback<MorphBallEyeSegment> Playback { get; set; }

    /// <summary>
    /// The pose the open eye looks along while it tracks Samus (#2070), chosen from its angle on each update it tracks; null
    /// while its eyelid or its mount animation shows instead.
    /// </summary>
    internal EnemyFrameId? Aim { get; set; }

    /// <summary>
    /// Whether the eye is activated: set once the beam object has initialized, cleared once
    /// the body starts closing. The beam keeps cycling its colours only while it is set.
    /// </summary>
    public bool IsActivated { get; internal set; }

    /// <summary>
    /// Zero-up, clockwise, 1/256-turn aim angle.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Activation/deactivation countdown.</summary>
    public int ActionTimer { get; internal set; }

    /// <summary>The action the eye performs.</summary>
    public MorphBallEyeAction Action { get; internal set; }
}
