using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Poses.SamusPoseAnimations;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Samus's pose animations: each frame's delay and the cue that ends each run (#1473).</summary>
/// <remarks>
/// An animation is the named segments a pose animates by (#2048), each closed by the cue it ends in, under a name of its own
/// (<see cref="SamusAnimationName"/>). It holds every segment of the game's animation: the frames the breathing, drained,
/// falling, spin and wall-jump animations reach by cue or by a frame their movement owner sets, and the segments only a
/// frame read past an earlier animation or a named place landing in it reaches. Animations no pose animates by are here
/// because such a read reaches them. These are gameplay cadence and transition definitions, not editable sprite artwork.
/// </remarks>
internal static class SamusAnimationDelayDefinitions
{
    /// <summary>Samus's energy below which the standing and crouching breathing continues into its low-energy breathing.</summary>
    internal const int LowEnergyBreathingBelow = 30;

    /// <summary>The updates each grapple swing picture shows for once the swing sets it from the rope's angle.</summary>
    internal const int GrappleSwingPictureHold = 15;

    /// <summary>The updates the neutral jump's rising picture is held for again each time it would end while Samus still rises.</summary>
    internal const int NeutralJumpRiseExtension = 4;

    /// <summary>The extra updates each animation picture of Samus is held for under water without the Gravity Suit.</summary>
    internal const int WaterAnimationDelay = 3;

    /// <summary>The extra updates each animation picture of Samus is held for in lava or acid without the Gravity Suit.</summary>
    internal const int LavaOrAcidAnimationDelay = 2;

    /// <summary>The animation <see cref="SamusPoseId.MovingRightNormalPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation RunningAnimation { get; } = new(SamusAnimationName.Running,
    [
        new(SamusAnimationSegmentName.Run, [2, 3, 2, 3, 2, 3, 2, 3, 2, 3], new SamusAnimationRestart()),
        new(SamusAnimationSegmentName.SlowStride, [.. Repeat(4, 6), 3, 4, 4, 3], new SamusAnimationRestart()),
        new(SamusAnimationSegmentName.StrideHold, [10], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.StandingAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation AimingUpAnimation { get; } = new(SamusAnimationName.AimingUp, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkFacingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MoonwalkAnimation { get; } = new(SamusAnimationName.Moonwalk, [new(SamusAnimationSegmentName.Main, [.. Repeat(16, 6)], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.NormalLandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalLandingRightAnimation { get; } = new(SamusAnimationName.NormalLandingRight, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalLandingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalLandingLeftAnimation { get; } = new(SamusAnimationName.NormalLandingLeft, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.SpinLandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation SpinLandingRightAnimation { get; } = new(SamusAnimationName.SpinLandingRight, [new(SamusAnimationSegmentName.Main, [3, 5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.SpinLandingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation SpinLandingLeftAnimation { get; } = new(SamusAnimationName.SpinLandingLeft, [new(SamusAnimationSegmentName.Main, [3, 5, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation LandingAimUpRightAnimation { get; } = new(SamusAnimationName.LandingAimUpRight, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation LandingAimUpLeftAnimation { get; } = new(SamusAnimationName.LandingAimUpLeft, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation LandingAimDiagonalUpRightAnimation { get; } = new(SamusAnimationName.LandingAimDiagonalUpRight, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation LandingAimDiagonalUpLeftAnimation { get; } = new(SamusAnimationName.LandingAimDiagonalUpLeft, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation LandingAimDiagonalDownRightAnimation { get; } = new(SamusAnimationName.LandingAimDiagonalDownRight, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.LandingAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation LandingAimDiagonalDownLeftAnimation { get; } = new(SamusAnimationName.LandingAimDiagonalDownLeft, [new(SamusAnimationSegmentName.Main, [5, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedCrouchingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DrainedCrouchingRightAnimation { get; } = new(SamusAnimationName.DrainedCrouchingRight,
    [
        new(SamusAnimationSegmentName.Collapse, [2, 2, 2, 16], new SamusAnimationInstallDrainedFall()),
        new(SamusAnimationSegmentName.Fall, [1], new SamusAnimationRepeat(SamusAnimationSegmentName.Fall, 0)),
        new(SamusAnimationSegmentName.Held, [16, 16, 16, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Held, 0)),
        new(SamusAnimationSegmentName.StandUp, [3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedCrouchingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DrainedCrouchingLeftAnimation { get; } = new(SamusAnimationName.DrainedCrouchingLeft,
    [
        new(SamusAnimationSegmentName.Collapse, [2, 2, 16], new SamusAnimationInstallDrainedFall()),
        new(SamusAnimationSegmentName.Fall, [1], new SamusAnimationRepeat(SamusAnimationSegmentName.Fall, 0)),
        new(SamusAnimationSegmentName.Held, [8, 16, 16, 16, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Held, 1)),
        new(SamusAnimationSegmentName.StandUp, [16, 16, 16], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose)),
        new(SamusAnimationSegmentName.FailedStand, [16, 16, 16, 16, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Held, 3)),
        new(SamusAnimationSegmentName.FailedStandEnd, [16], new SamusAnimationRepeat(SamusAnimationSegmentName.Held, 3)),
        new(SamusAnimationSegmentName.Frozen, [16], new SamusAnimationRepeat(SamusAnimationSegmentName.Frozen, 0)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedStandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DrainedStandingRightAnimation { get; } = new(SamusAnimationName.DrainedStandingRight,
    [
        new(SamusAnimationSegmentName.Breathe, [16, 16, 16, 16], new SamusAnimationRestart()),
        new(SamusAnimationSegmentName.StandUp, [3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DrainedStandingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DrainedStandingLeftAnimation { get; } = new(SamusAnimationName.DrainedStandingLeft,
    [
        new(SamusAnimationSegmentName.Breathe, [16, 16, 16, 16], new SamusAnimationRestart()),
        new(SamusAnimationSegmentName.StandUp, [3], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FacingRightNormalPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingAnimation { get; } = new(SamusAnimationName.Standing,
    [
        new(SamusAnimationSegmentName.Breathe, [10, 10, 10, 10], new SamusAnimationLoopUnlessLowEnergy()),
        new(SamusAnimationSegmentName.LowEnergyBreathe, [8, 8, 8, 8], new SamusAnimationRepeat(SamusAnimationSegmentName.LowEnergyBreathe, 0)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingAnimation { get; } = new(SamusAnimationName.Crouching,
    [
        new(SamusAnimationSegmentName.Breathe, [10, 10, 10, 10], new SamusAnimationLoopUnlessLowEnergy()),
        new(SamusAnimationSegmentName.LowEnergyBreathe, [8, 8, 8, 8], new SamusAnimationRepeat(SamusAnimationSegmentName.LowEnergyBreathe, 0)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.XrayingStandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation XrayingAnimation { get; } = new(SamusAnimationName.Xraying, [new(SamusAnimationSegmentName.Main, [15, 15, 15, 15, 15], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.StandingAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation HeldStillAnimation { get; } = new(SamusAnimationName.HeldStill, [new(SamusAnimationSegmentName.Main, [16], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.GrappleStandingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation GrappleStandingAnimation { get; } = new(SamusAnimationName.GrappleStanding, [new(SamusAnimationSegmentName.Main, [16], new SamusAnimationRestart())]);

    /// <summary>
    /// The right-facing airborne grapple aimed right animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation GrappleAirborneGunExtendedRightAnimation { get; } = new(SamusAnimationName.GrappleAirborneGunExtendedRight, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>
    /// The right-facing airborne grapple aimed down animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation GrappleAirborneAimDownRightAnimation { get; } = new(SamusAnimationName.GrappleAirborneAimDownRight, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>
    /// The right-facing airborne grapple aimed diagonally down animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation GrappleAirborneAimDiagonalDownRightAnimation { get; } = new(SamusAnimationName.GrappleAirborneAimDiagonalDownRight, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.GrappleSwingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation GrappleSwingAnimation { get; } = new(SamusAnimationName.GrappleSwing, [new(SamusAnimationSegmentName.Main, [.. Repeat(8, 66)], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 65))]);

    /// <summary>The animation <see cref="SamusPoseId.NeutralJumpTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NeutralJumpTransitionRightAnimation { get; } = new(SamusAnimationName.NeutralJumpTransitionRight, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NeutralJumpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NeutralJumpTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NeutralJumpTransitionLeftAnimation { get; } = new(SamusAnimationName.NeutralJumpTransitionLeft, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NeutralJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpTransitionAimUpRightAnimation { get; } = new(SamusAnimationName.NormalJumpTransitionAimUpRight, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpTransitionAimUpLeftAnimation { get; } = new(SamusAnimationName.NormalJumpTransitionAimUpLeft, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpTransitionAimDiagonalUpRightAnimation { get; } = new(SamusAnimationName.NormalJumpTransitionAimDiagonalUpRight, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpTransitionAimDiagonalUpLeftAnimation { get; } = new(SamusAnimationName.NormalJumpTransitionAimDiagonalUpLeft, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpTransitionAimDiagonalDownRightAnimation { get; } = new(SamusAnimationName.NormalJumpTransitionAimDiagonalDownRight, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpTransitionAimDiagonalDownLeftAnimation { get; } = new(SamusAnimationName.NormalJumpTransitionAimDiagonalDownLeft, [new(SamusAnimationSegmentName.Main, [1], new SamusAnimationTransition(SamusPoseId.NormalJumpAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.NeutralJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NeutralJumpAndWindupAnimation { get; } = new(SamusAnimationName.NeutralJumpAndWindup, [new(SamusAnimationSegmentName.Main, [3, 4, 4, 4, 4, 80], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 5))]);

    /// <summary>The animation <see cref="SamusPoseId.DamageBoostLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DamageBoostAnimation { get; } = new(SamusAnimationName.DamageBoost, [new(SamusAnimationSegmentName.Main, [8, .. Repeat(2, 9)], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 9))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpAimUpAnimation { get; } = new(SamusAnimationName.NormalJumpAimUp, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpAimDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpAimDownAnimation { get; } = new(SamusAnimationName.NormalJumpAimDown, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpForwardRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpForwardAnimation { get; } = new(SamusAnimationName.NormalJumpForward, [new(SamusAnimationSegmentName.Main, [2, 3], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.NormalJumpGunExtendedRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation NormalJumpAimingAnimation { get; } = new(SamusAnimationName.NormalJumpAiming, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.FallingRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation FallingAnimation { get; } = new(SamusAnimationName.Falling,
    [
        new(SamusAnimationSegmentName.FallStart, [8, 6, 6], new SamusAnimationRepeat(SamusAnimationSegmentName.FallStart, 2)),
        new(SamusAnimationSegmentName.FastFall, [8, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.FastFall, 1)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FallingGunExtendedRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation FallingGunExtendedAnimation { get; } = new(SamusAnimationName.FallingGunExtended,
    [
        new(SamusAnimationSegmentName.FallStart, [8, 6, 6], new SamusAnimationRepeat(SamusAnimationSegmentName.FallStart, 2)),
        new(SamusAnimationSegmentName.FastFall, [8, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.FastFall, 1)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FallingAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation FallingAimUpAnimation { get; } = new(SamusAnimationName.FallingAimUp, [new(SamusAnimationSegmentName.Main, [2, 16, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 2))]);

    /// <summary>The animation <see cref="SamusPoseId.FallingAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation FallingAimDiagonalAnimation { get; } = new(SamusAnimationName.FallingAimDiagonal,
    [
        new(SamusAnimationSegmentName.Aim, [2], new SamusAnimationHold()),
        new(SamusAnimationSegmentName.AimHeld, [16], new SamusAnimationRepeat(SamusAnimationSegmentName.AimHeld, 0)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.FallingAimDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation FallingAimDownAnimation { get; } = new(SamusAnimationName.FallingAimDown, [new(SamusAnimationSegmentName.Main, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.KnockbackRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation KnockbackAnimation { get; } = new(SamusAnimationName.Knockback,
    [
        new(SamusAnimationSegmentName.Knockback, [2, 16], new SamusAnimationRepeat(SamusAnimationSegmentName.Knockback, 1)),
        new(SamusAnimationSegmentName.RecoilHold, [10], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.MorphBallGroundRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MorphBallAnimation { get; } = new(SamusAnimationName.MorphBall,
    [
        new(SamusAnimationSegmentName.Roll, [.. Repeat(3, 8)], new SamusAnimationRestart()),
        new(SamusAnimationSegmentName.RollHold, [3], new SamusAnimationRepeat(SamusAnimationSegmentName.Roll, 0)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.SpinJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation SpinJumpAnimation { get; } = new(SamusAnimationName.SpinJump,
    [
        new(SamusAnimationSegmentName.Spin, [4, 3, 2, 3, 2, 3, 2, 3, 2], new SamusAnimationRepeat(SamusAnimationSegmentName.Spin, 1)),
        new(SamusAnimationSegmentName.WallJumpReady, [8], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.SpaceJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation SpaceJumpAnimation { get; } = new(SamusAnimationName.SpaceJump,
    [
        new(SamusAnimationSegmentName.Spin, [4, .. Repeat(1, 8)], new SamusAnimationRepeat(SamusAnimationSegmentName.Spin, 1)),
        new(SamusAnimationSegmentName.WallJumpReady, [8], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.ScrewAttackRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation ScrewAttackAnimation { get; } = new(SamusAnimationName.ScrewAttack,
    [
        new(SamusAnimationSegmentName.Spin, [4, .. Repeat(1, 24)], new SamusAnimationRepeat(SamusAnimationSegmentName.Spin, 1)),
        new(SamusAnimationSegmentName.WallJumpReady, [8], new SamusAnimationRestart()),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftAnimation { get; } = new(SamusAnimationName.TurningRightToLeft, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightAnimation { get; } = new(SamusAnimationName.TurningLeftToRight, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftJumpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftJump, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpForwardLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightJumpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightJump, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpForwardRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftCrouchingAnimation { get; } = new(SamusAnimationName.TurningRightToLeftCrouching, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightCrouchingAnimation { get; } = new(SamusAnimationName.TurningLeftToRightCrouching, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftFallingAnimation { get; } = new(SamusAnimationName.TurningRightToLeftFalling, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightFallingAnimation { get; } = new(SamusAnimationName.TurningLeftToRightFalling, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftAimUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightAimUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftAimDiagonalDownAnimation { get; } = new(SamusAnimationName.TurningRightToLeftAimDiagonalDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightAimDiagonalDownAnimation { get; } = new(SamusAnimationName.TurningLeftToRightAimDiagonalDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftJumpAimUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftJumpAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightJumpAimUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightJumpAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftJumpAimDownAnimation { get; } = new(SamusAnimationName.TurningRightToLeftJumpAimDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightJumpAimDownAnimation { get; } = new(SamusAnimationName.TurningLeftToRightJumpAimDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftFallingAimUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftFallingAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightFallingAimUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightFallingAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftFallingAimDownAnimation { get; } = new(SamusAnimationName.TurningRightToLeftFallingAimDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightFallingAimDownAnimation { get; } = new(SamusAnimationName.TurningLeftToRightFallingAimDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftCrouchingAimUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftCrouchingAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightCrouchingAimUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightCrouchingAimUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftCrouchingAimDiagonalDownAnimation { get; } = new(SamusAnimationName.TurningRightToLeftCrouchingAimDiagonalDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightCrouchingAimDiagonalDownAnimation { get; } = new(SamusAnimationName.TurningLeftToRightCrouchingAimDiagonalDown, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.StandingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftJumpAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftJumpAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightJumpAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightJumpAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.NormalJumpAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftFallingAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftFallingAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightFallingAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightFallingAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.FallingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningRightToLeftCrouchingAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningRightToLeftCrouchingAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation TurningLeftToRightCrouchingAimDiagonalUpAnimation { get; } = new(SamusAnimationName.TurningLeftToRightCrouchingAimDiagonalUp, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.CrouchingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MoonwalkTurnJumpLeftAnimation { get; } = new(SamusAnimationName.MoonwalkTurnJumpLeft, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MoonwalkTurnJumpRightAnimation { get; } = new(SamusAnimationName.MoonwalkTurnJumpRight, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MoonwalkTurnJumpAimUpLeftAnimation { get; } = new(SamusAnimationName.MoonwalkTurnJumpAimUpLeft, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MoonwalkTurnJumpAimUpRightAnimation { get; } = new(SamusAnimationName.MoonwalkTurnJumpAimUpRight, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MoonwalkTurnJumpAimDownLeftAnimation { get; } = new(SamusAnimationName.MoonwalkTurnJumpAimDownLeft, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MoonwalkTurnJumpAimDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MoonwalkTurnJumpAimDownRightAnimation { get; } = new(SamusAnimationName.MoonwalkTurnJumpAimDownRight, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTurnTransition(SamusPoseId.SpinJumpRightPose))]);

    /// <summary>
    /// The left-facing Draygon grab animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation DraygonGrabbingLeftAnimation { get; } = new(SamusAnimationName.DraygonGrabbingLeft, [new(SamusAnimationSegmentName.Main, [2, 2, 2], new SamusAnimationTransition(SamusPoseId.DraygonGrabbedNeutralLeftPose))]);

    /// <summary>
    /// The right-facing compact airborne animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation CompactAirborneRightAnimation { get; } = new(SamusAnimationName.CompactAirborneRight, [new(SamusAnimationSegmentName.Main, [4, 3], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>
    /// The left-facing compact airborne animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation CompactAirborneLeftAnimation { get; } = new(SamusAnimationName.CompactAirborneLeft, [new(SamusAnimationSegmentName.Main, [4, 3], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>
    /// The compact airborne fall animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation CompactAirborneFallingAnimation { get; } = new(SamusAnimationName.CompactAirborneFalling, [new(SamusAnimationSegmentName.Main, [3, .. Repeat(2, 8)], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 1))]);

    /// <summary>The animation <see cref="SamusPoseId.WallJumpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation WallJumpAnimation { get; } = new(SamusAnimationName.WallJump,
    [
        new(SamusAnimationSegmentName.Launch, [5, 5], new SamusAnimationWallJumpSelect(new(SamusAnimationSegmentName.OrdinarySpin, 0), new(SamusAnimationSegmentName.SpaceJumpSpin, 0), new(SamusAnimationSegmentName.ScrewAttackSpin, 0))),
        new(SamusAnimationSegmentName.OrdinarySpin, [3, 2, 3, 2, 3, 2, 3, 2], new SamusAnimationRepeat(SamusAnimationSegmentName.OrdinarySpin, 0)),
        new(SamusAnimationSegmentName.SpaceJumpSpin, [2, 1, 2, 1, 2, 1, 2, 1], new SamusAnimationRepeat(SamusAnimationSegmentName.SpaceJumpSpin, 0)),
        new(SamusAnimationSegmentName.ScrewAttackSpin, [.. Repeat(1, 24)], new SamusAnimationRepeat(SamusAnimationSegmentName.ScrewAttackSpin, 0)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionRightAnimation { get; } = new(SamusAnimationName.CrouchingTransitionRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionLeftAnimation { get; } = new(SamusAnimationName.CrouchingTransitionLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MorphingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MorphingTransitionRightAnimation { get; } = new(SamusAnimationName.MorphingTransitionRight, [new(SamusAnimationSegmentName.Main, [3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.MorphingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation MorphingTransitionLeftAnimation { get; } = new(SamusAnimationName.MorphingTransitionLeft, [new(SamusAnimationSegmentName.Main, [3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.MorphBallFallingLeftPose, SamusPoseId.SpringBallGroundLeftPose, SamusPoseId.SpringBallFallingLeftPose))]);

    /// <summary>
    /// The right-facing glitch-ball morph animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation GlitchBallMorphingRightAnimation { get; } = new(SamusAnimationName.GlitchBallMorphingRight, [new(SamusAnimationSegmentName.Main, [0], new SamusAnimationTransition(SamusPoseId.GlitchBallRightPose))]);

    /// <summary>
    /// The left-facing glitch-ball morph animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation GlitchBallMorphingLeftAnimation { get; } = new(SamusAnimationName.GlitchBallMorphingLeft, [new(SamusAnimationSegmentName.Main, [0], new SamusAnimationTransition(SamusPoseId.GlitchBallLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionRightAnimation { get; } = new(SamusAnimationName.StandingTransitionRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionLeftAnimation { get; } = new(SamusAnimationName.StandingTransitionLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnmorphingTransitionRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation UnmorphingTransitionRightAnimation { get; } = new(SamusAnimationName.UnmorphingTransitionRight, [new(SamusAnimationSegmentName.Main, [3, 3], new SamusAnimationTransition(SamusPoseId.CrouchingRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.UnmorphingTransitionLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation UnmorphingTransitionLeftAnimation { get; } = new(SamusAnimationName.UnmorphingTransitionLeft, [new(SamusAnimationSegmentName.Main, [3, 3], new SamusAnimationTransition(SamusPoseId.CrouchingLeftPose))]);

    /// <summary>
    /// The right-facing instant morph animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation InstantMorphingRightAnimation { get; } = new(SamusAnimationName.InstantMorphingRight, [new(SamusAnimationSegmentName.Main, [0], new SamusAnimationItemTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.SpringBallGroundRightPose))]);

    /// <summary>
    /// The left-facing instant morph animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation InstantMorphingLeftAnimation { get; } = new(SamusAnimationName.InstantMorphingLeft, [new(SamusAnimationSegmentName.Main, [0], new SamusAnimationItemTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.SpringBallGroundLeftPose))]);

    /// <summary>
    /// The right-facing compact morph animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation CompactMorphingRightAnimation { get; } = new(SamusAnimationName.CompactMorphingRight, [new(SamusAnimationSegmentName.Main, [3, 3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.MorphBallGroundRightPose, SamusPoseId.MorphBallFallingRightPose, SamusPoseId.SpringBallGroundRightPose, SamusPoseId.SpringBallFallingRightPose))]);

    /// <summary>
    /// The left-facing Draygon-held morph animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation DraygonHeldMorphingLeftAnimation { get; } = new(SamusAnimationName.DraygonHeldMorphingLeft, [new(SamusAnimationSegmentName.Main, [3, 3, 3], new SamusAnimationItemAirborneTransition(SamusEquipmentFlags.SpringBall, SamusPoseId.DraygonHeldBallLeftPose, SamusPoseId.DraygonHeldBallLeftPose, SamusPoseId.DraygonHeldBallLeftPose, SamusPoseId.DraygonHeldBallLeftPose))]);

    /// <summary>
    /// The right-facing compact stand-up animation, which no pose Samus holds animates by: only a read past an animation laid out before
    /// it reaches it, under the reading pose (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal static SamusPoseAnimation CompactStandingUpRightAnimation { get; } = new(SamusAnimationName.CompactStandingUpRight, [new(SamusAnimationSegmentName.Main, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose))]);

    /// <summary>The animation <see cref="SamusPoseId.DraygonHeldStandingUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DraygonHeldStandingUpLeftAnimation { get; } = new(SamusAnimationName.DraygonHeldStandingUpLeft, [new(SamusAnimationSegmentName.Main, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.DraygonGrabbedNeutralLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionAimUpRightAnimation { get; } = new(SamusAnimationName.CrouchingTransitionAimUpRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionAimUpLeftAnimation { get; } = new(SamusAnimationName.CrouchingTransitionAimUpLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionAimDiagonalUpRightAnimation { get; } = new(SamusAnimationName.CrouchingTransitionAimDiagonalUpRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionAimDiagonalUpLeftAnimation { get; } = new(SamusAnimationName.CrouchingTransitionAimDiagonalUpLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionAimDiagonalDownRightAnimation { get; } = new(SamusAnimationName.CrouchingTransitionAimDiagonalDownRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrouchingTransitionAimDiagonalDownLeftAnimation { get; } = new(SamusAnimationName.CrouchingTransitionAimDiagonalDownLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.CrouchingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionAimUpRightAnimation { get; } = new(SamusAnimationName.StandingTransitionAimUpRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.StandingAimUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionAimUpLeftAnimation { get; } = new(SamusAnimationName.StandingTransitionAimUpLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.StandingAimUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalUpRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionAimDiagonalUpRightAnimation { get; } = new(SamusAnimationName.StandingTransitionAimDiagonalUpRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalUpRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalUpLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionAimDiagonalUpLeftAnimation { get; } = new(SamusAnimationName.StandingTransitionAimDiagonalUpLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalUpLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalDownRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionAimDiagonalDownRightAnimation { get; } = new(SamusAnimationName.StandingTransitionAimDiagonalDownRight, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalDownRightPose))]);

    /// <summary>The animation <see cref="SamusPoseId.StandingTransitionAimDiagonalDownLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation StandingTransitionAimDiagonalDownLeftAnimation { get; } = new(SamusAnimationName.StandingTransitionAimDiagonalDownLeft, [new(SamusAnimationSegmentName.Main, [3], new SamusAnimationTransition(SamusPoseId.StandingAimDiagonalDownLeftPose))]);

    /// <summary>The animation <see cref="SamusPoseId.DraygonGrabbedMovingLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DraygonGrabbedMovingAnimation { get; } = new(SamusAnimationName.DraygonGrabbedMoving, [new(SamusAnimationSegmentName.Main, [.. Repeat(6, 6)], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.ShinesparkHorizontalRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation ShinesparkAnimation { get; } = new(SamusAnimationName.Shinespark, [new(SamusAnimationSegmentName.Main, [8], new SamusAnimationRestart())]);

    /// <summary>The animation <see cref="SamusPoseId.CrystalFlashRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrystalFlashRightAnimation { get; } = new(SamusAnimationName.CrystalFlashRight,
    [
        new(SamusAnimationSegmentName.Rise, [3, 3, 1, 1], new SamusAnimationRepeat(SamusAnimationSegmentName.Rise, 2)),
        new(SamusAnimationSegmentName.Glow, [12, 12, 12, 12], new SamusAnimationRepeat(SamusAnimationSegmentName.Glow, 0)),
        new(SamusAnimationSegmentName.Finish, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.FacingRightNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.CrystalFlashLeftPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation CrystalFlashLeftAnimation { get; } = new(SamusAnimationName.CrystalFlashLeft,
    [
        new(SamusAnimationSegmentName.Rise, [3, 3, 1, 1], new SamusAnimationRepeat(SamusAnimationSegmentName.Rise, 2)),
        new(SamusAnimationSegmentName.Glow, [12, 12, 12, 12], new SamusAnimationRepeat(SamusAnimationSegmentName.Glow, 0)),
        new(SamusAnimationSegmentName.Finish, [3, 3, 3], new SamusAnimationTransition(SamusPoseId.FacingLeftNormalPose)),
    ]);

    /// <summary>The animation <see cref="SamusPoseId.DeathSequenceRightPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation DeathSequenceAnimation { get; } = new(SamusAnimationName.DeathSequence, [new(SamusAnimationSegmentName.Main, [.. Repeat(2, 6)], new SamusAnimationRepeat(SamusAnimationSegmentName.Main, 5))]);

    /// <summary>The animation <see cref="SamusPoseId.ForwardFacingPowerSuitPose"/> plays, shared by every pose <see cref="SamusAnimationDelays.AnimationForPose"/> maps to it.</summary>
    internal static SamusPoseAnimation ForwardFacingAnimation { get; } = new(SamusAnimationName.ForwardFacing,
    [
        new(SamusAnimationSegmentName.Still, [8], new SamusAnimationRestart()),
        new(SamusAnimationSegmentName.Appearance, [.. Repeat(3, 89), 51, 2, 2, 2, 48], new SamusAnimationRepeat(SamusAnimationSegmentName.Appearance, 93)),
    ]);

    /// <summary>Every animation, one for each <see cref="SamusAnimationName"/>, in the order of their names.</summary>
    internal static IReadOnlyList<SamusPoseAnimation> All { get; } =
    [
        RunningAnimation,
        AimingUpAnimation,
        MoonwalkAnimation,
        NormalLandingRightAnimation,
        NormalLandingLeftAnimation,
        SpinLandingRightAnimation,
        SpinLandingLeftAnimation,
        LandingAimUpRightAnimation,
        LandingAimUpLeftAnimation,
        LandingAimDiagonalUpRightAnimation,
        LandingAimDiagonalUpLeftAnimation,
        LandingAimDiagonalDownRightAnimation,
        LandingAimDiagonalDownLeftAnimation,
        DrainedCrouchingRightAnimation,
        DrainedCrouchingLeftAnimation,
        DrainedStandingRightAnimation,
        DrainedStandingLeftAnimation,
        StandingAnimation,
        CrouchingAnimation,
        XrayingAnimation,
        HeldStillAnimation,
        GrappleStandingAnimation,
        GrappleAirborneGunExtendedRightAnimation,
        GrappleAirborneAimDownRightAnimation,
        GrappleAirborneAimDiagonalDownRightAnimation,
        GrappleSwingAnimation,
        NeutralJumpTransitionRightAnimation,
        NeutralJumpTransitionLeftAnimation,
        NormalJumpTransitionAimUpRightAnimation,
        NormalJumpTransitionAimUpLeftAnimation,
        NormalJumpTransitionAimDiagonalUpRightAnimation,
        NormalJumpTransitionAimDiagonalUpLeftAnimation,
        NormalJumpTransitionAimDiagonalDownRightAnimation,
        NormalJumpTransitionAimDiagonalDownLeftAnimation,
        NeutralJumpAndWindupAnimation,
        DamageBoostAnimation,
        NormalJumpAimUpAnimation,
        NormalJumpAimDownAnimation,
        NormalJumpForwardAnimation,
        NormalJumpAimingAnimation,
        FallingAnimation,
        FallingGunExtendedAnimation,
        FallingAimUpAnimation,
        FallingAimDiagonalAnimation,
        FallingAimDownAnimation,
        KnockbackAnimation,
        MorphBallAnimation,
        SpinJumpAnimation,
        SpaceJumpAnimation,
        ScrewAttackAnimation,
        TurningRightToLeftAnimation,
        TurningLeftToRightAnimation,
        TurningRightToLeftJumpAnimation,
        TurningLeftToRightJumpAnimation,
        TurningRightToLeftCrouchingAnimation,
        TurningLeftToRightCrouchingAnimation,
        TurningRightToLeftFallingAnimation,
        TurningLeftToRightFallingAnimation,
        TurningRightToLeftAimUpAnimation,
        TurningLeftToRightAimUpAnimation,
        TurningRightToLeftAimDiagonalDownAnimation,
        TurningLeftToRightAimDiagonalDownAnimation,
        TurningRightToLeftJumpAimUpAnimation,
        TurningLeftToRightJumpAimUpAnimation,
        TurningRightToLeftJumpAimDownAnimation,
        TurningLeftToRightJumpAimDownAnimation,
        TurningRightToLeftFallingAimUpAnimation,
        TurningLeftToRightFallingAimUpAnimation,
        TurningRightToLeftFallingAimDownAnimation,
        TurningLeftToRightFallingAimDownAnimation,
        TurningRightToLeftCrouchingAimUpAnimation,
        TurningLeftToRightCrouchingAimUpAnimation,
        TurningRightToLeftCrouchingAimDiagonalDownAnimation,
        TurningLeftToRightCrouchingAimDiagonalDownAnimation,
        TurningRightToLeftAimDiagonalUpAnimation,
        TurningLeftToRightAimDiagonalUpAnimation,
        TurningRightToLeftJumpAimDiagonalUpAnimation,
        TurningLeftToRightJumpAimDiagonalUpAnimation,
        TurningRightToLeftFallingAimDiagonalUpAnimation,
        TurningLeftToRightFallingAimDiagonalUpAnimation,
        TurningRightToLeftCrouchingAimDiagonalUpAnimation,
        TurningLeftToRightCrouchingAimDiagonalUpAnimation,
        MoonwalkTurnJumpLeftAnimation,
        MoonwalkTurnJumpRightAnimation,
        MoonwalkTurnJumpAimUpLeftAnimation,
        MoonwalkTurnJumpAimUpRightAnimation,
        MoonwalkTurnJumpAimDownLeftAnimation,
        MoonwalkTurnJumpAimDownRightAnimation,
        DraygonGrabbingLeftAnimation,
        CompactAirborneRightAnimation,
        CompactAirborneLeftAnimation,
        CompactAirborneFallingAnimation,
        WallJumpAnimation,
        CrouchingTransitionRightAnimation,
        CrouchingTransitionLeftAnimation,
        MorphingTransitionRightAnimation,
        MorphingTransitionLeftAnimation,
        GlitchBallMorphingRightAnimation,
        GlitchBallMorphingLeftAnimation,
        StandingTransitionRightAnimation,
        StandingTransitionLeftAnimation,
        UnmorphingTransitionRightAnimation,
        UnmorphingTransitionLeftAnimation,
        InstantMorphingRightAnimation,
        InstantMorphingLeftAnimation,
        CompactMorphingRightAnimation,
        DraygonHeldMorphingLeftAnimation,
        CompactStandingUpRightAnimation,
        DraygonHeldStandingUpLeftAnimation,
        CrouchingTransitionAimUpRightAnimation,
        CrouchingTransitionAimUpLeftAnimation,
        CrouchingTransitionAimDiagonalUpRightAnimation,
        CrouchingTransitionAimDiagonalUpLeftAnimation,
        CrouchingTransitionAimDiagonalDownRightAnimation,
        CrouchingTransitionAimDiagonalDownLeftAnimation,
        StandingTransitionAimUpRightAnimation,
        StandingTransitionAimUpLeftAnimation,
        StandingTransitionAimDiagonalUpRightAnimation,
        StandingTransitionAimDiagonalUpLeftAnimation,
        StandingTransitionAimDiagonalDownRightAnimation,
        StandingTransitionAimDiagonalDownLeftAnimation,
        DraygonGrabbedMovingAnimation,
        ShinesparkAnimation,
        CrystalFlashRightAnimation,
        CrystalFlashLeftAnimation,
        DeathSequenceAnimation,
        ForwardFacingAnimation,
    ];
}
