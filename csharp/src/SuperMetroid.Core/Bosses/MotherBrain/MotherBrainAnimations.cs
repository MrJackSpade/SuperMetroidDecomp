using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodySegment;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadSegment;
using M = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's typed body and brain animations (#641, #1600).</summary>
internal static class MotherBrainAnimations
{
    /// <summary>Every body segment.</summary>
    internal static AnimationSegments<B, EnemyFrameId, MotherBrainBodyCue> BodySegments { get; } = new(
        new Dictionary<B, AnimationBeat<EnemyFrameId, MotherBrainBodyCue>[]>
    {
        [B.WalkForwardReallyFast] = WalkForward(2),
        [B.WalkForwardFast] = WalkForward(4),
        [B.WalkForwardMedium] = WalkForward(6),
        [B.WalkForwardSlow] = WalkForward(8),
        [B.WalkForwardReallySlow] = WalkForward(10),
        [B.WalkBackwardSlow] = WalkBackward(8),
        [B.WalkBackwardReallyFast] = WalkBackward(2),
        [B.WalkBackwardFast] = WalkBackward(4),
        [B.WalkBackwardMedium] = WalkBackward(6),
        [B.WalkBackwardReallySlow] = WalkBackward(10),
        [B.CrouchAndThenStandUp] =
        [
            .. Crouch(8, 8),
            BodyShow(EnemyFrameId.MotherBrainBodyCrouched, 8),
            BodyCue(new MotherBrainBodyCue.Move(new(0, -10))),
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(EnemyFrameId.MotherBrainBodyUncrouching, 8),
            BodyCue(new MotherBrainBodyCue.Move(new(0, -16))),
            BodyShow(EnemyFrameId.MotherBrainBodyLeaningDown, 8),
            BodyCue(new MotherBrainBodyCue.Move(new(0, -12))),
            BodyShow(EnemyFrameId.MotherBrainBodyStanding, 8),
            Pose(MotherBrainBodyPose.Standing),
        ],
        [B.StandAfterCrouchSlow] = [.. StandUp(16)],
        [B.StandAfterCrouchFast] = [.. StandUp(8)],
        [B.StandAfterLeaning] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(EnemyFrameId.MotherBrainBodyLeaningDown, 8),
            BodyCue(new MotherBrainBodyCue.Move(new(0, -12))),
            BodyShow(EnemyFrameId.MotherBrainBodyStanding, 8),
            Pose(MotherBrainBodyPose.Standing),
        ],
        [B.LeanDown] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(EnemyFrameId.MotherBrainBodyStanding, 8),
            BodyCue(new MotherBrainBodyCue.Move(new(0, 12))),
            Pose(MotherBrainBodyPose.LeaningDown),
            BodyShow(EnemyFrameId.MotherBrainBodyLeaningDown, 8),
        ],
        [B.Crouched] =
        [
            Pose(MotherBrainBodyPose.Crouched),
            BodyShow(EnemyFrameId.MotherBrainBodyCrouched, 8),
        ],
        [B.CrouchSlow] = [.. Crouch(8, 8)],
        [B.CrouchFast] = [.. Crouch(2, 2)],
        [B.HandBeam] = HandBeam(),
    });

    /// <summary>Each body segment's animation: its segment played once, then held.</summary>
    internal static IReadOnlyDictionary<B, Animation<B>> BodyStartingAt { get; } =
        Enum.GetValues<B>().ToDictionary(segment => segment, segment => new Animation<B>([BodySegments.Whole(segment)], AnimationEnd.Holds));

    /// <summary>The body animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<B> BodyStartingWith(B segment) => BodyStartingAt[segment];

    /// <summary>A forward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue>[] WalkForward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(EnemyFrameId.MotherBrainBodyStanding, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingNearLegLifting, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(2, 0))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingNearLegReaching, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(0, 1))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting, duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(3, 1))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingLegsApart, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(15, -2))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingFarLegLifting, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(6, -4))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingFarLegPassing, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-2, 4))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting, duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingLegsTogether, duration),
    ];

    /// <summary>A backward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue>[] WalkBackward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingLegsTogether, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(2, -4))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingFarLegPassing, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-6, 4))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingFarLegLifting, duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(-15, 2))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingLegsApart, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-3, -1))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(0, -1))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingNearLegReaching, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-2, 0))),
        BodyShow(EnemyFrameId.MotherBrainBodyWalkingNearLegLifting, duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(EnemyFrameId.MotherBrainBodyStanding, duration),
    ];

    /// <summary>Lowers the body into the crouch; the lean and uncrouch frames last the given updates.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue>[] Crouch(int lean, int uncrouch) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(EnemyFrameId.MotherBrainBodyStanding, 8),
        BodyCue(new MotherBrainBodyCue.Move(new(0, 12))),
        BodyShow(EnemyFrameId.MotherBrainBodyLeaningDown, lean),
        BodyCue(new MotherBrainBodyCue.Move(new(0, 16))),
        BodyShow(EnemyFrameId.MotherBrainBodyUncrouching, uncrouch),
        BodyCue(new MotherBrainBodyCue.Move(new(0, 10))),
        Pose(MotherBrainBodyPose.Crouched),
        BodyShow(EnemyFrameId.MotherBrainBodyCrouched, 8),
    ];

    /// <summary>Raises the body from the crouch to standing, each frame lasting <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue>[] StandUp(int duration) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(EnemyFrameId.MotherBrainBodyCrouched, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(0, -10))),
        BodyShow(EnemyFrameId.MotherBrainBodyUncrouching, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(0, -16))),
        BodyShow(EnemyFrameId.MotherBrainBodyLeaningDown, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(0, -12))),
        BodyShow(EnemyFrameId.MotherBrainBodyStanding, duration),
        Pose(MotherBrainBodyPose.Standing),
    ];

    /// <summary>
    /// The red hand beam: the arm rises, eight charge particles alternate between the particle and the energy spark at the
    /// hand, each followed by the arm drawn back, and the beam fires; the body holds the standing pose before the next
    /// phase. The arm drawn back shows twice running before the beam fires: her body shows each new frame of its own on her
    /// body layer, so the second showing is a frame of its own (#2318).
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue>[] HandBeam() =>
    [
        Pose(MotherBrainBodyPose.DeathBeam),
        BodyShow(EnemyFrameId.MotherBrainBodyStanding, 1),
        BodyShow(EnemyFrameId.MotherBrainBodyHandCurling, 1),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeParticle),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeEnergy),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeParticle),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeEnergy),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeParticle),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeEnergy),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeParticle),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(ChargeEnergy),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        BodyCue(MotherBrainBodyCue.FireHandBeam),
        BodyShow(EnemyFrameId.MotherBrainBodyArmFlungOut, 1),
        BodyShow(EnemyFrameId.MotherBrainBodyArmReturning, 1),
        BodyShow(EnemyFrameId.MotherBrainBodyStanding, 240),
        BodyCue(MotherBrainBodyCue.AdvanceHandBeamPhase),
        Pose(MotherBrainBodyPose.Standing),
    ];

    /// <summary>The hand beam's charge particle, at her elbow.</summary>
    private static MotherBrainBodyCue ChargeParticle => new MotherBrainBodyCue.SpawnHandDust(new(36, -40), DustExplosionAnimation.MotherBrainElbowChargeParticle);

    /// <summary>The hand beam's charge energy spark, at her elbow.</summary>
    private static MotherBrainBodyCue ChargeEnergy => new MotherBrainBodyCue.SpawnHandDust(new(34, -42), DustExplosionAnimation.MotherBrainElbowChargeEnergy);

    /// <summary>Shows a body frame.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue> BodyShow(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBodyCue>.Frame(frame, duration);

    /// <summary>Raises a body event.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue> BodyCue(MotherBrainBodyCue cue) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBodyCue>.Event(cue);

    /// <summary>Raises a body pose.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue> Pose(MotherBrainBodyPose pose) =>
        BodyCue(new MotherBrainBodyCue.SetPose(pose));

    /// <summary>
    /// Every brain segment, as its list shows it (#2318). Nothing waits on her brain showing a new frame, so a picture its
    /// list shows twice running with nothing between is one frame of both: the Hyper Beam recoil opens her mouth as the
    /// stretch does without drooling, so its wide and its widest mouth each show as one frame. Her still pictures hold.
    /// </summary>
    internal static AnimationSegments<H, EnemyFrameId, MotherBrainHeadCue> HeadSegments { get; } = new(
        new Dictionary<H, AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[]>
    {
        [H.StretchingPhase2] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 3),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 3),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 3),
            Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 17),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 17),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 33),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 5),
        ],
        [H.StretchingPhase2Held] = [Held(EnemyFrameId.MotherBrainHeadEyeGlaring)],
        [H.StretchingPhase3] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 3),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 3),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 3),
            Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 17),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 17),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 33),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 5),
        ],
        [H.StretchingPhase3Held] = [Held(EnemyFrameId.MotherBrainDarkHeadEyeGlaring)],
        [H.HyperBeamRecoil] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 3),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 6),
            Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 67),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 5),
        ],
        [H.HyperBeamRecoilHeld] = [Held(EnemyFrameId.MotherBrainDarkHeadEyeGlaring)],
        [H.WholeHeadMouthClosed] = [Held(EnemyFrameId.MotherBrainHeadEyeShut)],
        [H.Decapitated] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 9), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 5)],
        [H.DecapitatedHeld] = [Held(EnemyFrameId.MotherBrainDarkHeadEyeShut)],
        [H.DyingDrool] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 5),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 5),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
        ],
        [H.DyingDroolBurst] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
        ],
        [H.DyingDroolLoop] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.MaybeDroolAgain),
        ],
        [H.FiringRainbowBeam] = [Held(EnemyFrameId.MotherBrainHeadEyeGlaring)],
        [H.NeutralPhase2] = [Show(EnemyFrameId.MotherBrainHeadEyeShut, 5), Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5), Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 9), Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5), Show(EnemyFrameId.MotherBrainHeadEyeShut, 5), Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5)],
        [H.NeutralPhase2Hold] =
        [
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 9),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase2),
            Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5),
        ],
        [H.NeutralPhase3] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 5), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 5), Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 9), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 5), Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 5), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 5)],
        [H.NeutralPhase3Hold] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 9),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 9),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase3),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 5),
        ],
        [H.Corpse] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 3), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 3), Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 65), Show(EnemyFrameId.MotherBrainHeadCorpseSlumping, 65)],
        [H.CorpseHeld] = [Held(EnemyFrameId.MotherBrainHeadCorpseSlumped)],
        [H.FourOnionRingsPhase2] =
        [
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 5),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 5),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 9),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 4),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 4),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 4),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 17),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 5),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 17),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.TwoOnionRingsPhase2] =
        [
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 5),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 5),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 9),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 4),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 17),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 5),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 17),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.AttackingBabyMetroid] =
        [
            Cue(MotherBrainHeadCueKind.CountBabyMetroidAttack),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid),
        ],
        [H.AttackingSamusFourOnionRingsPhase3] =
        [
            Cue(MotherBrainHeadCueKind.ResetBabyMetroidAttacks),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
        ],
        [H.FourOnionRingsPhase3] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 5),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 5),
            Cue(MotherBrainHeadCueKind.QueueBabyMetroidAttackSound),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 9),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 4),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 4),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 4),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 17),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 5),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 17),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.BombPhase2] = Bomb(WholeHead, afterburns: 7),
        [H.BombPhase3] = Bomb(DamagedHead, afterburns: 1),
        [H.Laser] =
        [
            Show(EnemyFrameId.MotherBrainHeadEyeOpen, 17),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 5),
            Cue(MotherBrainHeadCueKind.SpawnLaser),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 33),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.ChargingRainbowBeam] =
        [
            Cue(MotherBrainHeadCueKind.PrepareRainbowBeamCharge),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 5),
            Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5),
            Show(EnemyFrameId.MotherBrainHeadEyeShut, 3),
        ],
        [H.ChargingRainbowBeamLoop] =
        [
            Cue(MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle),
            Show(EnemyFrameId.MotherBrainHeadEyeShut, 31),
        ],
    });

    /// <summary>
    /// The brain animations, by the segment each starts at (#1948): each lists its own segments, played once in turn,
    /// and the one it loops from, or rests on the still picture it ends at (#2353). Only the segments she starts an animation at, or a random hold or drool continues at,
    /// start one; the segments an animation only runs on into are listed in it and start none.
    /// </summary>
    internal static IReadOnlyDictionary<H, Animation<H>> HeadStartingAt { get; } = new Dictionary<H, Animation<H>>
    {
        [H.StretchingPhase2] = Resting(H.StretchingPhase2, H.StretchingPhase2Held),
        [H.StretchingPhase3] = Resting(H.StretchingPhase3, H.StretchingPhase3Held),
        [H.HyperBeamRecoil] = Resting(H.HyperBeamRecoil, H.HyperBeamRecoilHeld),
        [H.WholeHeadMouthClosed] = Resting(H.WholeHeadMouthClosed),
        [H.Decapitated] = Resting(H.Decapitated, H.DecapitatedHeld),
        [H.DyingDrool] = Looping(2, H.DyingDrool, H.DyingDroolBurst, H.DyingDroolLoop),
        [H.DyingDroolBurst] = Looping(1, H.DyingDroolBurst, H.DyingDroolLoop),
        [H.FiringRainbowBeam] = Resting(H.FiringRainbowBeam),
        [H.NeutralPhase2] = Looping(0, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.NeutralPhase2Hold] = Looping(0, H.NeutralPhase2Hold, H.NeutralPhase2),
        [H.NeutralPhase3] = Looping(0, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.NeutralPhase3Hold] = Looping(0, H.NeutralPhase3Hold, H.NeutralPhase3),
        [H.Corpse] = Resting(H.Corpse, H.CorpseHeld),
        [H.FourOnionRingsPhase2] = Looping(1, H.FourOnionRingsPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.TwoOnionRingsPhase2] = Looping(1, H.TwoOnionRingsPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.AttackingBabyMetroid] = Looping(2, H.AttackingBabyMetroid, H.FourOnionRingsPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.AttackingSamusFourOnionRingsPhase3] = Looping(2, H.AttackingSamusFourOnionRingsPhase3, H.FourOnionRingsPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.BombPhase2] = Looping(1, H.BombPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.BombPhase3] = Looping(1, H.BombPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.Laser] = Looping(1, H.Laser, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.ChargingRainbowBeam] = Looping(1, H.ChargingRainbowBeam, H.ChargingRainbowBeamLoop),
    };

    /// <summary>The brain animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<H> HeadStartingWith(H segment) => HeadStartingAt[segment];

    /// <summary>
    /// A bomb lob from <paramref name="head"/>: phase two lobs a seven-afterburn bomb from the whole head, phase three a
    /// one-afterburn bomb from the damaged one.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] Bomb(HeadMouth head, int afterburns) =>
    [
        Show(head.Closed, 5),
        Show(head.Opening, 5),
        Show(head.Open, 9),
        Cue(MotherBrainHeadCueKind.DisableNeckMovement),
        Show(head.Open, 5),
        Show(head.Wide, 5),
        HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
        Show(head.Widest, 9),
        HeadCue(MotherBrainHeadCue.Bomb(afterburns)),
        Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
        Show(head.Widest, 33),
        Show(head.Wide, 5),
        Show(head.Open, 17),
        Cue(MotherBrainHeadCueKind.EnableNeckMovement),
    ];

    /// <summary>A head's frames at each stage of opening its mouth, from closed to widest.</summary>
    /// <param name="Closed">The closed mouth.</param>
    /// <param name="Opening">The mouth starting to open.</param>
    /// <param name="Open">The open mouth.</param>
    /// <param name="Wide">The wide-open mouth.</param>
    /// <param name="Widest">The widest-open mouth.</param>
    private sealed record HeadMouth(M Closed, M Opening, M Open, M Wide, M Widest);

    /// <summary>The whole head of phase two.</summary>
    private static HeadMouth WholeHead => new(EnemyFrameId.MotherBrainHeadEyeShut, EnemyFrameId.MotherBrainHeadEyeOpen, EnemyFrameId.MotherBrainHeadEyeGlaring,
        EnemyFrameId.MotherBrainHeadJawDropping, EnemyFrameId.MotherBrainHeadJawDropped);

    /// <summary>The damaged head of phase three.</summary>
    private static HeadMouth DamagedHead => new(EnemyFrameId.MotherBrainDarkHeadEyeShut, EnemyFrameId.MotherBrainDarkHeadEyeOpen, EnemyFrameId.MotherBrainDarkHeadEyeGlaring,
        EnemyFrameId.MotherBrainDarkHeadJawDropping, EnemyFrameId.MotherBrainDarkHeadJawDropped);

    /// <summary>
    /// Shows brain frame <paramref name="frame"/> for <paramref name="updates"/> updates (#2317), the updates it shows at
    /// her brain's draws.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> Show(EnemyFrameId frame, int updates) =>
        new AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame(frame, updates);

    /// <summary>Shows brain frame <paramref name="frame"/> for good: the still picture her brain rests on (#2318).</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> Held(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.HeldFrame(frame);

    /// <summary>Raises a brain event that carries no values.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> Cue(MotherBrainHeadCueKind kind) =>
        HeadCue(MotherBrainHeadCue.Of(kind));

    /// <summary>Raises a brain event.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> HeadCue(MotherBrainHeadCue cue) =>
        new AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Event(cue);

    /// <summary>An animation of <paramref name="segments"/>, each played once in turn, that loops from phase <paramref name="loopsFrom"/>.</summary>
    private static Animation<H> Looping(int loopsFrom, params H[] segments) => new([.. segments.Select(HeadSegments.Whole)], AnimationEnd.LoopsFrom(loopsFrom));

    /// <summary>
    /// An animation of <paramref name="segments"/>, each played once in turn, that rests on the still picture the last one
    /// holds (#2353): her brain shows it for good, until she plays another.
    /// </summary>
    private static Animation<H> Resting(params H[] segments) => new([.. segments.Select(HeadSegments.Whole)], AnimationEnd.Holds);
}
