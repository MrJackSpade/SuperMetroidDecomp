using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Poses.SamusAnimationSegmentName;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The animation frames the drained movement and the drain's operations set directly, outside the drained poses' own
/// animations (#1513, #2048). Each is a named frame of the left-facing drained crouch or stand, which the game sets on
/// whatever drained pose Samus holds, or, while she cannot stand and after the drain has let her go, on any pose: there it
/// lands on the frame the import decoded for the place in that pose's animation (<see cref="SamusAnimationDelays.Land"/>). Several name a
/// segment's end, whose next frame is the first of the segment after it.
/// </summary>
internal static class SamusDrainedAnimationDefinitions
{
    /// <summary>The drained pose the get-up starts from and moves on Up: the left-facing drained crouch.</summary>
    internal const SamusPoseId GetUpPose = SamusPoseId.DrainedCrouchingLeftPose;

    /// <summary>The held crouch's first frame after its landing, from which the crouch accepts Up once she can stand.</summary>
    internal static SamusAnimationPlace AbleToStandFirstFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedAbleToStandFirst, Held, 0);
    /// <summary>The held crouch's end, which the get-up moves the left-facing drained crouch to on Up once she can stand: its next frame starts the stand-up.</summary>
    internal static SamusAnimationPlace AbleToStandFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedAbleToStand, HeldReturn, 3);
    /// <summary>The first frame from which the drained crouch accepts Up while she cannot stand, whatever pose Samus holds.</summary>
    internal static SamusAnimationPlace UnableToStandFirstFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedUnableToStandFirst, Held, 0);
    /// <summary>The last frame from which the drained crouch accepts Up while she cannot stand.</summary>
    internal static SamusAnimationPlace UnableToStandLastFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedUnableToStandLast, HeldReturn, 1);
    /// <summary>The stand-up's end, which the get-up moves the animation to on Up while she cannot stand: its next frame starts her failed attempt to stand.</summary>
    internal static SamusAnimationPlace UnableToStandFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedUnableToStand, StandUp, 4);
    /// <summary>The failed stand's end, which the Hyper Beam acquisition moves the drained animation to: its next frame is the frozen crouch it holds.</summary>
    internal static SamusAnimationPlace HyperBeamAcquisitionFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedHyperBeamAcquisition, FailedStandEnd, 2);
    /// <summary>The held crouch's end, from which the rainbow stop starts the standing part of the drained animation: its next frame starts the stand-up.</summary>
    internal static SamusAnimationPlace RainbowStopFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedRainbowStop, HeldReturn, 3);
    /// <summary>The held crouch's landing frame, which a falling drained crouch lands on.</summary>
    internal static SamusAnimationPlace LandingFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedLanding, Landing, 0);
    /// <summary>How many ticks the landing frame is held.</summary>
    internal const int LandingTimer = 8;

    /// <summary>
    /// The timer every other frame the drained sequence sets starts with: Samus's animation steps on at her next animation
    /// update (#2280).
    /// </summary>
    internal const int SetFrameTimer = 1;
    /// <summary>The held crouch's end, which the release moves a drained crouch to: its next frame starts the stand-up.</summary>
    internal static SamusAnimationPlace ReleaseCrouchingFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedReleaseCrouching, HeldReturn, 3);
    /// <summary>The drained breathing's restart, which the release moves a drained standing pose to.</summary>
    internal static SamusAnimationPlace ReleaseStandingFrame { get; } = SamusAnimationDelayDefinitions.DrainedStandingLeftAnimation.Place(SamusAnimationPlaceName.DrainedReleaseStanding, Breathe, 4);
    /// <summary>The collapse's third frame, which the drained fall initializes the drained crouch on.</summary>
    internal static SamusAnimationPlace FallInitialFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedFallInitial, Collapse, 2);
    /// <summary>The held crouch's first frame after its landing, which the drained crouch or fall writes the drained crouch on.</summary>
    internal static SamusAnimationPlace PutCrouchingFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedPutCrouching, Held, 0);
    /// <summary>How many ticks the drained crouch or fall holds its first frame.</summary>
    internal const int PutCrouchingTimer = 16;
    /// <summary>How many ticks standing drained Samus up holds the drained stand's first frame.</summary>
    internal const int PutStandingTimer = 16;
    /// <summary>
    /// The drained crouch's vertical radius, in pixels: letting drained Samus fall keeps her feet where they were as her
    /// body takes it.
    /// </summary>
    internal const int CrouchingYRadius = 21;
}
