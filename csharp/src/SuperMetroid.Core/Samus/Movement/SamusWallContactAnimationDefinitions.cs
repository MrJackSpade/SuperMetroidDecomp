using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Spin-jump animation frames that wall contact rewinds to.</summary>
internal static class SamusWallContactAnimationDefinitions
{
    /// <summary>
    /// Screw Attack's rewind frame: its longer pre-contact animation makes frame 27 the first
    /// eligible wall frame, so contact rewinds to 26.
    /// </summary>
    internal static SamusAnimationPlace ScrewAttackRewindFrame { get; } = SamusAnimationDelayDefinitions.ScrewAttackAnimation.Place(SamusAnimationPlaceName.ScrewAttackWallContactRewind, SamusAnimationSegmentName.Spin, 26);

    /// <summary>Ordinary spin and Space Jump's rewind frame, the compact 10 -> 11 handoff.</summary>
    internal static SamusAnimationPlace SpinJumpRewindFrame { get; } = SamusAnimationDelayDefinitions.SpinJumpAnimation.Place(SamusAnimationPlaceName.SpinJumpWallContactRewind, SamusAnimationSegmentName.Spin, 10);
}
