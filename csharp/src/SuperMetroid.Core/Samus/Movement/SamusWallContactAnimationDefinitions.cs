using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Spin-jump animation frames that wall contact rewinds to.</summary>
internal static class SamusWallContactAnimationDefinitions
{
    /// <summary>
    /// Screw Attack's rewind frame: the end of its longer spin, whose next frame is its first wall-jump-ready frame
    /// (<see cref="SamusAnimationFrames.ScrewAttackWallJumpReady"/>).
    /// </summary>
    internal static SamusAnimationPlace ScrewAttackRewindFrame { get; } = SamusAnimationDelayDefinitions.ScrewAttackAnimation.Place(SamusAnimationPlaceName.ScrewAttackWallContactRewind, SamusAnimationSegmentName.Spin, 26);

    /// <summary>
    /// Ordinary spin and Space Jump's rewind frame: the end of the spin, whose next frame is its first wall-jump-ready frame
    /// (<see cref="SamusAnimationFrames.SpinJumpWallJumpReady"/>).
    /// </summary>
    internal static SamusAnimationPlace SpinJumpRewindFrame { get; } = SamusAnimationDelayDefinitions.SpinJumpAnimation.Place(SamusAnimationPlaceName.SpinJumpWallContactRewind, SamusAnimationSegmentName.Spin, 10);
}
