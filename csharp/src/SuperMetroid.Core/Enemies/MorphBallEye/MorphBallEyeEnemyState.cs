using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The state of the Morph Ball eye. The mount and eye body share a definition, but parameter two's sign selects their
/// role.
/// </summary>
public sealed class MorphBallEyeEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MorphBallEyeSegment> Playback { get; set; }

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
    public int FunctionTimer { get; internal set; }

    /// <summary>The function the eye runs.</summary>
    public MorphBallEyeAiFunction? Function { get; internal set; }
}
