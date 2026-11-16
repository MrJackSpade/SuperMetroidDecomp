using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Every named place of Samus's animations (#2048), one for each <see cref="SamusAnimationPlaceName"/>: the frames paths
/// outside an animation set or compare her animation with. The import decodes where each lands in every other animation.
/// </summary>
internal static class SamusAnimationPlaces
{
    /// <summary>Every named place, in the order of their names.</summary>
    internal static IReadOnlyList<SamusAnimationPlace> All { get; } =
    [
        SamusAnimationFrames.NeutralJumpRise,
        SamusAnimationFrames.SpaceJumpSpinStart,
        SamusAnimationFrames.SpaceJumpSpinMiddle,
        SamusAnimationFrames.WallJumpOrdinarySpinStart,
        SamusAnimationFrames.WallJumpSpaceJumpSpinStart,
        SamusAnimationFrames.WallJumpScrewAttackSpinStart,
        SamusAnimationFrames.SpinJumpWallJumpReady,
        SamusAnimationFrames.ScrewAttackWallJumpReady,
        SamusAnimationFrames.ScrewAttackSpinStart,
        SamusAnimationFrames.FastFallStart,
        SamusDrainedAnimationDefinitions.AbleToStandFirstFrame,
        SamusDrainedAnimationDefinitions.AbleToStandFrame,
        SamusDrainedAnimationDefinitions.UnableToStandFirstFrame,
        SamusDrainedAnimationDefinitions.UnableToStandLastFrame,
        SamusDrainedAnimationDefinitions.UnableToStandFrame,
        SamusDrainedAnimationDefinitions.HyperBeamAcquisitionFrame,
        SamusDrainedAnimationDefinitions.RainbowStopFrame,
        SamusDrainedAnimationDefinitions.LandingFrame,
        SamusDrainedAnimationDefinitions.ReleaseCrouchingFrame,
        SamusDrainedAnimationDefinitions.ReleaseStandingFrame,
        SamusDrainedAnimationDefinitions.FallInitialFrame,
        SamusDrainedAnimationDefinitions.PutCrouchingFrame,
        SamusWallContactAnimationDefinitions.ScrewAttackRewindFrame,
        SamusWallContactAnimationDefinitions.SpinJumpRewindFrame,
    ];
}
