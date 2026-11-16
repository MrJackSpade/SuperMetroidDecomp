using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The state of a Morph Ball eye's beam mount (#2164): only the mount pose it shows.</summary>
public sealed class MorphBallEyeMountState
{
    /// <summary>The mount animation the mount plays and where it stands in it (#1600), which its placing starts.</summary>
    internal AnimationPlayback<MorphBallEyeSegment> Playback { get; set; }
}
