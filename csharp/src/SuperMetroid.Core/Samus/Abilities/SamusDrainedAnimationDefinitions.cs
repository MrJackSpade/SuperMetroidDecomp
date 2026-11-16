using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Poses.SamusAnimationSegmentName;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The animation frames the drained movement and the drain's operations set directly, outside the drained poses' own
/// animations (#1513, #2048). Each is a named frame of the left-facing drained crouch or stand, which the game sets on
/// whatever drained pose Samus holds, or, while she cannot stand and after the drain has let her go, on any pose: there it
/// lands on the frame the import decoded for the place in that pose's animation (<see cref="SamusAnimationDelays.Land"/>). Several name a
/// value a cue carries, which the game reads as a step of its own.
/// </summary>
internal static class SamusDrainedAnimationDefinitions
{
    /// <summary>The held crouch's first frame after its landing, from which the crouch accepts Up once she can stand.</summary>
    internal static SamusAnimationPlace AbleToStandFirstFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedAbleToStandFirst, Held, 1);
    /// <summary>The value the held crouch's repeat carries, which the get-up moves the left-facing drained crouch to on Up once she can stand, starting its stand-up.</summary>
    internal static SamusAnimationPlace AbleToStandFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedAbleToStand, Held, 6);
    /// <summary>The first frame from which the drained crouch accepts Up while she cannot stand, whatever pose Samus holds.</summary>
    internal static SamusAnimationPlace UnableToStandFirstFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedUnableToStandFirst, Held, 1);
    /// <summary>The last frame from which the drained crouch accepts Up while she cannot stand.</summary>
    internal static SamusAnimationPlace UnableToStandLastFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedUnableToStandLast, Held, 4);
    /// <summary>The value the stand-up's transition carries, which the get-up moves the animation to on Up while she cannot stand, its failed attempt to stand.</summary>
    internal static SamusAnimationPlace UnableToStandFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedUnableToStand, StandUp, 4);
    /// <summary>The value the failed stand's last repeat carries, at which the Hyper Beam acquisition freezes the drained animation.</summary>
    internal static SamusAnimationPlace HyperBeamAcquisitionFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedHyperBeamAcquisition, FailedStandEnd, 2);
    /// <summary>The value the held crouch's repeat carries, from which the rainbow stop starts the standing part of the drained animation.</summary>
    internal static SamusAnimationPlace RainbowStopFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedRainbowStop, Held, 6);
    /// <summary>The held crouch's landing frame, which a falling drained crouch lands on.</summary>
    internal static SamusAnimationPlace LandingFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedLanding, Held, 0);
    /// <summary>How many ticks the landing frame is held.</summary>
    internal const int LandingTimer = 8;

    /// <summary>
    /// The timer every other frame the drained sequence sets starts with: Samus's animation steps on at her next animation
    /// update (#2280).
    /// </summary>
    internal const int SetFrameTimer = 1;
    /// <summary>The value the held crouch's repeat carries, which the release moves a drained crouch to.</summary>
    internal static SamusAnimationPlace ReleaseCrouchingFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedReleaseCrouching, Held, 6);
    /// <summary>The drained breathing's restart, which the release moves a drained standing pose to.</summary>
    internal static SamusAnimationPlace ReleaseStandingFrame { get; } = SamusAnimationDelayDefinitions.DrainedStandingLeftAnimation.Place(SamusAnimationPlaceName.DrainedReleaseStanding, Breathe, 4);
    /// <summary>The collapse's third frame, which the drained-fall controller initializes the drained crouch on.</summary>
    internal static SamusAnimationPlace FallInitialFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedFallInitial, Collapse, 2);
    /// <summary>The held crouch's first frame after its landing, which the crouching-or-falling controller writes the drained crouch on.</summary>
    internal static SamusAnimationPlace PutCrouchingFrame { get; } = SamusAnimationDelayDefinitions.DrainedCrouchingLeftAnimation.Place(SamusAnimationPlaceName.DrainedPutCrouching, Held, 1);
    /// <summary>How many ticks the crouching-or-falling controller holds its first frame.</summary>
    internal const int PutCrouchingTimer = 16;
    /// <summary>How many ticks standing drained Samus up holds the drained stand's first frame.</summary>
    internal const int PutStandingTimer = 16;
    /// <summary>
    /// The drained crouch's vertical radius, in pixels: letting drained Samus fall keeps her feet where they were as her
    /// body takes it.
    /// </summary>
    internal const int CrouchingYRadius = 21;
}
