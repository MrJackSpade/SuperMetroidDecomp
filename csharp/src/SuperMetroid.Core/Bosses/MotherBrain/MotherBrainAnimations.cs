using SuperMetroid.Core.Animation;
using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodySegment;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadSegment;
using M = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

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
        Enum.GetValues<B>().ToDictionary(segment => segment, segment => new Animation<B>([Whole(segment)], AnimationEnd.Holds));

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
    /// The red hand beam: the arm rises, eight charge particles alternate between the particle and the energy
    /// spark at the hand, the beam fires, and the body holds the standing pose before the next phase.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue>[] HandBeam()
    {
        var steps = new List<AnimationBeat<EnemyFrameId, MotherBrainBodyCue>>
        {
            Pose(MotherBrainBodyPose.DeathBeam),
            BodyShow(EnemyFrameId.MotherBrainBodyStanding, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyHandCurling, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        };
        for (int particle = 0; particle < 8; particle++)
        {
            steps.Add(BodyCue(particle % 2 == 0
                ? new MotherBrainBodyCue.SpawnHandDust(new(36, -40), MiscDustAnimation.MotherBrainElbowChargeParticle)
                : new MotherBrainBodyCue.SpawnHandDust(new(34, -42), MiscDustAnimation.MotherBrainElbowChargeEnergy)));
            steps.Add(BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1));
        }
        steps.AddRange(
        [
            BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
            BodyCue(MotherBrainBodyCue.FireHandBeam),
            BodyShow(EnemyFrameId.MotherBrainBodyArmFlungOut, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyArmReturning, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyStanding, 240),
            BodyCue(MotherBrainBodyCue.AdvanceHandBeamPhase),
            Pose(MotherBrainBodyPose.Standing),
        ]);
        return [.. steps];
    }

    /// <summary>Shows a body frame.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue> BodyShow(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBodyCue>.Frame(frame, duration);

    /// <summary>Raises a body event.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue> BodyCue(MotherBrainBodyCue cue) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBodyCue>.Event(cue);

    /// <summary>Raises a body pose.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBodyCue> Pose(MotherBrainBodyPose pose) =>
        BodyCue(new MotherBrainBodyCue.SetPose(pose));

    /// <summary>Every brain segment.</summary>
    internal static AnimationSegments<H, EnemyFrameId, MotherBrainHeadCue> HeadSegments { get; } = new(
        new Dictionary<H, AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[]>
    {
        [H.StretchingPhase2] = [.. Stretch(WholeHead, MotherBrainStretchKind.Attack)],
        [H.StretchingPhase2Loop] = [Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 1)],
        [H.StretchingPhase3] = [.. Stretch(DamagedHead, MotherBrainStretchKind.Attack)],
        [H.StretchingPhase3Loop] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 1)],
        [H.HyperBeamRecoil] = [.. Stretch(DamagedHead, MotherBrainStretchKind.HyperBeamRecoil)],
        [H.HyperBeamRecoilLoop] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 1)],
        [H.Initial] = [Show(EnemyFrameId.MotherBrainHeadEyeShut, 4)],
        [H.Decapitated] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 8), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 4)],
        [H.DecapitatedLoop] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 4)],
        [H.DyingDrool] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 4),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 4),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
        ],
        [H.DyingDroolBurst] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
        ],
        [H.DyingDroolLoop] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 2),
            Cue(MotherBrainHeadCueKind.MaybeDroolAgain),
        ],
        [H.FiringRainbowBeam] = [Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 1)],
        [H.NeutralPhase2] = [Show(EnemyFrameId.MotherBrainHeadEyeShut, 4), Show(EnemyFrameId.MotherBrainHeadEyeOpen, 4), Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 8), Show(EnemyFrameId.MotherBrainHeadEyeOpen, 4), Show(EnemyFrameId.MotherBrainHeadEyeShut, 4), Show(EnemyFrameId.MotherBrainHeadEyeOpen, 4)],
        [H.NeutralPhase2Hold] =
        [
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase2),
            Show(EnemyFrameId.MotherBrainHeadEyeOpen, 4),
        ],
        [H.NeutralPhase3] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 4), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 4), Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 8), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 4), Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 4), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 4)],
        [H.NeutralPhase3Hold] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 8),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase3),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 4),
        ],
        [H.Corpse] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 2), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 2), Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 64), Show(EnemyFrameId.MotherBrainHeadCorpseSlumping, 64)],
        [H.CorpseLoop] = [Show(EnemyFrameId.MotherBrainHeadCorpseSlumped, 2)],
        [H.FourOnionRingsPhase2] = OnionRingsPhase2(rings: 4),
        [H.TwoOnionRingsPhase2] = OnionRingsPhase2(rings: 2),
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
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 4),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 4),
            Cue(MotherBrainHeadCueKind.QueueBabyMetroidAttackSound),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 8),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 16),
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 4),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.BombPhase2] = Bomb(WholeHead, afterburns: 7),
        [H.BombPhase3] = Bomb(DamagedHead, afterburns: 1),
        [H.Laser] =
        [
            Show(EnemyFrameId.MotherBrainHeadEyeOpen, 16),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 4),
            Cue(MotherBrainHeadCueKind.SpawnLaser),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 32),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.ChargingRainbowBeam] =
        [
            Cue(MotherBrainHeadCueKind.PrepareRainbowBeamCharge),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 4),
            Show(EnemyFrameId.MotherBrainHeadEyeOpen, 4),
            Show(EnemyFrameId.MotherBrainHeadEyeShut, 2),
        ],
        [H.ChargingRainbowBeamLoop] =
        [
            Cue(MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle),
            Show(EnemyFrameId.MotherBrainHeadEyeShut, 30),
        ],
    });

    /// <summary>
    /// The brain animations, by the segment each starts at (#1948): each lists its own segments, played once in turn,
    /// and the one it loops from. Only the segments she starts an animation at, or a random hold or drool continues at,
    /// start one; the segments an animation only runs on into are listed in it and start none.
    /// </summary>
    internal static IReadOnlyDictionary<H, Animation<H>> HeadStartingAt { get; } = new Dictionary<H, Animation<H>>
    {
        [H.StretchingPhase2] = Looping(1, H.StretchingPhase2, H.StretchingPhase2Loop),
        [H.StretchingPhase3] = Looping(1, H.StretchingPhase3, H.StretchingPhase3Loop),
        [H.HyperBeamRecoil] = Looping(1, H.HyperBeamRecoil, H.HyperBeamRecoilLoop),
        [H.Initial] = Looping(0, H.Initial),
        [H.Decapitated] = Looping(1, H.Decapitated, H.DecapitatedLoop),
        [H.DyingDrool] = Looping(2, H.DyingDrool, H.DyingDroolBurst, H.DyingDroolLoop),
        [H.DyingDroolBurst] = Looping(1, H.DyingDroolBurst, H.DyingDroolLoop),
        [H.FiringRainbowBeam] = Looping(0, H.FiringRainbowBeam),
        [H.NeutralPhase2] = Looping(0, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.NeutralPhase2Hold] = Looping(0, H.NeutralPhase2Hold, H.NeutralPhase2),
        [H.NeutralPhase3] = Looping(0, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.NeutralPhase3Hold] = Looping(0, H.NeutralPhase3Hold, H.NeutralPhase3),
        [H.Corpse] = Looping(1, H.Corpse, H.CorpseLoop),
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
    /// The neck stretch and the Hyper Beam recoil: the brain shakes, <paramref name="head"/>'s mouth opens through its open,
    /// wide and widest stages, breathing a big purple cloud with the stretch cry, and drools while stretching.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] Stretch(HeadMouth head, MotherBrainStretchKind kind)
    {
        var steps = new List<AnimationBeat<EnemyFrameId, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(head.Open, 2),
            Show(head.Wide, 2),
        };
        if (kind == MotherBrainStretchKind.Attack) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
        steps.Add(Show(head.Wide, 2));
        steps.Add(Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath));
        steps.Add(HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)));
        foreach (int duration in new[] { 16, 16, 32 })
        {
            if (kind == MotherBrainStretchKind.Attack) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
            steps.Add(Show(head.Widest, duration));
        }
        steps.Add(Show(head.Wide, 4));
        return [.. steps];
    }

    /// <summary>A phase-two onion-ring volley of <paramref name="rings"/> rings aimed at Samus.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] OnionRingsPhase2(int rings)
    {
        var steps = new List<AnimationBeat<EnemyFrameId, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 4),
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 4),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
            Show(EnemyFrameId.MotherBrainHeadJawDropped, 8),
        };
        for (int ring = 0; ring < rings; ring++)
        {
            steps.Add(Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus));
            steps.Add(Cue(MotherBrainHeadCueKind.SpawnOnionRing));
            if (ring == 0) steps.Add(HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)));
            steps.Add(Show(EnemyFrameId.MotherBrainHeadJawDropped, ring == rings - 1 ? 16 : 3));
        }
        steps.AddRange(
        [
            Show(EnemyFrameId.MotherBrainHeadJawDropping, 4),
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ]);
        return [.. steps];
    }

    /// <summary>
    /// A bomb lob from <paramref name="head"/>: phase two lobs a seven-afterburn bomb from the whole head, phase three a
    /// one-afterburn bomb from the damaged one.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] Bomb(HeadMouth head, int afterburns) =>
    [
        Show(head.Closed, 4),
        Show(head.Opening, 4),
        Show(head.Open, 8),
        Cue(MotherBrainHeadCueKind.DisableNeckMovement),
        Show(head.Open, 4),
        Show(head.Wide, 4),
        HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
        Show(head.Widest, 8),
        HeadCue(MotherBrainHeadCue.Bomb(afterburns)),
        Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
        Show(head.Widest, 32),
        Show(head.Wide, 4),
        Show(head.Open, 16),
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

    /// <summary>Shows brain frame <paramref name="frame"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame(frame, duration);

    /// <summary>Raises a brain event that carries no values.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> Cue(MotherBrainHeadCueKind kind) =>
        HeadCue(MotherBrainHeadCue.Of(kind));

    /// <summary>Raises a brain event.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue> HeadCue(MotherBrainHeadCue cue) =>
        new AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Event(cue);

    /// <summary>One play of every beat of body segment <paramref name="segment"/>.</summary>
    private static AnimationPhase<B> Whole(B segment) => new(segment, 0, BodySegments.Beats(segment).Count, 1);

    /// <summary>One play of every beat of brain segment <paramref name="segment"/>.</summary>
    private static AnimationPhase<H> Whole(H segment) => new(segment, 0, HeadSegments.Beats(segment).Count, 1);

    /// <summary>An animation of <paramref name="segments"/>, each played once in turn, that loops from phase <paramref name="loopsFrom"/>.</summary>
    private static Animation<H> Looping(int loopsFrom, params H[] segments) => new([.. segments.Select(segment => Whole(segment))], AnimationEnd.LoopsFrom(loopsFrom));
}
