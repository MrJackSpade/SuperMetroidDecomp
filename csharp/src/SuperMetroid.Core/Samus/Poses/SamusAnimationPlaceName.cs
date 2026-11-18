namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The named frames of Samus's animations that paths outside an animation set, or compare her animation with, whatever
/// animation she animates by (#2048). Each names one <see cref="SamusAnimationPlace"/>; where Samus animates by another
/// animation, the place lands on the frame the import decoded for it in that animation
/// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}.Landing"/>).
/// </summary>
internal enum SamusAnimationPlaceName
{
    /// <summary>The neutral jump's rising frame (<see cref="SamusAnimationFrames.NeutralJumpRise"/>).</summary>
    NeutralJumpRise,

    /// <summary>The Space Jump spin's first frame (<see cref="SamusAnimationFrames.SpaceJumpSpinStart"/>).</summary>
    SpaceJumpSpinStart,

    /// <summary>The Space Jump spin's ninth frame (<see cref="SamusAnimationFrames.SpaceJumpSpinMiddle"/>).</summary>
    SpaceJumpSpinMiddle,

    /// <summary>The ordinary wall-jump spin's first frame (<see cref="SamusAnimationFrames.WallJumpOrdinarySpinStart"/>).</summary>
    WallJumpOrdinarySpinStart,

    /// <summary>The Space Jump wall-jump spin's first frame (<see cref="SamusAnimationFrames.WallJumpSpaceJumpSpinStart"/>).</summary>
    WallJumpSpaceJumpSpinStart,

    /// <summary>The Screw Attack wall-jump spin's first frame (<see cref="SamusAnimationFrames.WallJumpScrewAttackSpinStart"/>).</summary>
    WallJumpScrewAttackSpinStart,

    /// <summary>The spin's first frame a wall jump can launch from (<see cref="SamusAnimationFrames.SpinJumpWallJumpReady"/>).</summary>
    SpinJumpWallJumpReady,

    /// <summary>The Screw Attack spin's first frame a wall jump can launch from (<see cref="SamusAnimationFrames.ScrewAttackWallJumpReady"/>).</summary>
    ScrewAttackWallJumpReady,

    /// <summary>The Screw Attack spin's first frame (<see cref="SamusAnimationFrames.ScrewAttackSpinStart"/>).</summary>
    ScrewAttackSpinStart,

    /// <summary>The fast fall's first frame as a place in the fall's animation (<see cref="SamusAnimationFrames.FastFallStart"/>).</summary>
    FastFallStart,

    /// <summary>The held drained crouch's first frame after its landing, once she can stand (<see cref="Abilities.SamusDrainedAnimationDefinitions.AbleToStandFirstFrame"/>).</summary>
    DrainedAbleToStandFirst,

    /// <summary>The drained get-up's stand-up once she can stand (<see cref="Abilities.SamusDrainedAnimationDefinitions.AbleToStandFrame"/>).</summary>
    DrainedAbleToStand,

    /// <summary>The first frame from which the drained crouch accepts Up while she cannot stand (<see cref="Abilities.SamusDrainedAnimationDefinitions.UnableToStandFirstFrame"/>).</summary>
    DrainedUnableToStandFirst,

    /// <summary>The last frame from which the drained crouch accepts Up while she cannot stand (<see cref="Abilities.SamusDrainedAnimationDefinitions.UnableToStandLastFrame"/>).</summary>
    DrainedUnableToStandLast,

    /// <summary>The drained get-up's failed attempt to stand (<see cref="Abilities.SamusDrainedAnimationDefinitions.UnableToStandFrame"/>).</summary>
    DrainedUnableToStand,

    /// <summary>Where the Hyper Beam acquisition freezes the drained animation (<see cref="Abilities.SamusDrainedAnimationDefinitions.HyperBeamAcquisitionFrame"/>).</summary>
    DrainedHyperBeamAcquisition,

    /// <summary>Where the rainbow stop starts the standing part of the drained animation (<see cref="Abilities.SamusDrainedAnimationDefinitions.RainbowStopFrame"/>).</summary>
    DrainedRainbowStop,

    /// <summary>The held drained crouch's landing frame (<see cref="Abilities.SamusDrainedAnimationDefinitions.LandingFrame"/>).</summary>
    DrainedLanding,

    /// <summary>Where the release moves a drained crouch (<see cref="Abilities.SamusDrainedAnimationDefinitions.ReleaseCrouchingFrame"/>).</summary>
    DrainedReleaseCrouching,

    /// <summary>Where the release moves a drained standing pose (<see cref="Abilities.SamusDrainedAnimationDefinitions.ReleaseStandingFrame"/>).</summary>
    DrainedReleaseStanding,

    /// <summary>The collapse's frame the drained fall starts the drained crouch on (<see cref="Abilities.SamusDrainedAnimationDefinitions.FallInitialFrame"/>).</summary>
    DrainedFallInitial,

    /// <summary>The held crouch's frame the drained crouch or fall writes (<see cref="Abilities.SamusDrainedAnimationDefinitions.PutCrouchingFrame"/>).</summary>
    DrainedPutCrouching,

    /// <summary>The Screw Attack spin's frame wall contact rewinds to (<see cref="Movement.SamusWallContactAnimationDefinitions.ScrewAttackRewindFrame"/>).</summary>
    ScrewAttackWallContactRewind,

    /// <summary>The spin's frame wall contact rewinds to (<see cref="Movement.SamusWallContactAnimationDefinitions.SpinJumpRewindFrame"/>).</summary>
    SpinJumpWallContactRewind,
}
