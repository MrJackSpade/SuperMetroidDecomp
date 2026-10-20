using SuperMetroid.Core.Programs;
using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodyProgram;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadProgram;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's typed body and brain programs (#641), in place of their bank-$A9 instruction lists.</summary>
internal static class MotherBrainAnimations
{
    /// <summary>Every body program.</summary>
    internal static AnimationProgramSet<B, MotherBrainBodyFrame, MotherBrainBodyCue> Body { get; } = new(
        new Dictionary<B, AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[]>
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
            BodyShow(MotherBrainBodyFrame.Crouched, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-10, 4)),
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.Uncrouching, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-16, 4)),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            Pose(MotherBrainBodyPose.Standing),
            BodyHold,
        ],
        [B.StandAfterCrouchSlow] = [.. StandUp(16), BodyHold],
        [B.StandAfterCrouchFast] = [.. StandUp(8), BodyHold],
        [B.StandAfterLeaning] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            Pose(MotherBrainBodyPose.Standing),
            BodyHold,
        ],
        [B.LeanDown] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            BodyCue(MotherBrainBodyCue.MoveWithScrollBias(12, 4)),
            Pose(MotherBrainBodyPose.LeaningDown),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyHold,
        ],
        [B.Crouched] =
        [
            Pose(MotherBrainBodyPose.Crouched),
            BodyShow(MotherBrainBodyFrame.Crouched, 8),
            BodyHold,
        ],
        [B.CrouchSlow] = [.. Crouch(8, 8), BodyHold],
        [B.CrouchFast] = [.. Crouch(2, 2), BodyHold],
        [B.HandBeam] = HandBeam(),
        [B.InitialDummy] =
        [
            BodyShow(MotherBrainBodyFrame.InitialDummy, 0),
            BodyHold,
        ],
    });

    /// <summary>A forward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] WalkForward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        BodyCue(MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(0), duration),
        BodyCue(MotherBrainBodyCue.Move(new(2, 0))),
        BodyShow(MotherBrainBodyFrame.Walking(1), duration),
        BodyCue(MotherBrainBodyCue.Move(new(0, 1))),
        BodyShow(MotherBrainBodyFrame.Walking(2), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(3, 1))),
        BodyShow(MotherBrainBodyFrame.Walking(3), duration),
        BodyCue(MotherBrainBodyCue.Move(new(15, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(4), duration),
        BodyCue(MotherBrainBodyCue.Move(new(6, -4))),
        BodyShow(MotherBrainBodyFrame.Walking(5), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-2, 4))),
        BodyShow(MotherBrainBodyFrame.Walking(6), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(MotherBrainBodyFrame.Walking(7), duration),
        BodyHold,
    ];

    /// <summary>A backward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] WalkBackward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(MotherBrainBodyFrame.Walking(7), duration),
        BodyCue(MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(6), duration),
        BodyCue(MotherBrainBodyCue.Move(new(2, -4))),
        BodyShow(MotherBrainBodyFrame.Walking(5), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-6, 4))),
        BodyShow(MotherBrainBodyFrame.Walking(4), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(-15, 2))),
        BodyShow(MotherBrainBodyFrame.Walking(3), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-3, -1))),
        BodyShow(MotherBrainBodyFrame.Walking(2), duration),
        BodyCue(MotherBrainBodyCue.Move(new(0, -1))),
        BodyShow(MotherBrainBodyFrame.Walking(1), duration),
        BodyCue(MotherBrainBodyCue.Move(new(-2, 0))),
        BodyShow(MotherBrainBodyFrame.Walking(0), duration),
        BodyCue(MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        BodyHold,
    ];

    /// <summary>Lowers the body into the crouch; the lean and uncrouch frames last the given updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] Crouch(int lean, int uncrouch) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(MotherBrainBodyFrame.Standing, 8),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(12, 4)),
        BodyShow(MotherBrainBodyFrame.LeaningDown, lean),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(16, -2)),
        BodyShow(MotherBrainBodyFrame.Uncrouching, uncrouch),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(10, -2)),
        Pose(MotherBrainBodyPose.Crouched),
        BodyShow(MotherBrainBodyFrame.Crouched, 8),
    ];

    /// <summary>Raises the body from the crouch to standing, each frame lasting <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] StandUp(int duration) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(MotherBrainBodyFrame.Crouched, duration),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-10, 4)),
        BodyShow(MotherBrainBodyFrame.Uncrouching, duration),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-16, 4)),
        BodyShow(MotherBrainBodyFrame.LeaningDown, duration),
        BodyCue(MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        Pose(MotherBrainBodyPose.Standing),
    ];

    /// <summary>
    /// The red hand beam: the arm rises, eight charge particles alternate between the particle and the energy
    /// spark at the hand, the beam fires, and the body holds the standing pose before the next phase.
    /// </summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>[] HandBeam()
    {
        var steps = new List<AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>>
        {
            Pose(MotherBrainBodyPose.DeathBeam),
            BodyShow(MotherBrainBodyFrame.Standing, 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(0), 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(1), 1),
        };
        for (int particle = 0; particle < 8; particle++)
        {
            steps.Add(BodyCue(particle % 2 == 0
                ? MotherBrainBodyCue.SpawnHandDust(new(36, -40), MiscDustAnimation.MotherBrainElbowChargeParticle)
                : MotherBrainBodyCue.SpawnHandDust(new(34, -42), MiscDustAnimation.MotherBrainElbowChargeEnergy)));
            steps.Add(BodyShow(MotherBrainBodyFrame.HandBeam(1), 1));
        }
        steps.AddRange(
        [
            BodyShow(MotherBrainBodyFrame.HandBeam(1), 1),
            BodyCue(MotherBrainBodyCue.FireHandBeam),
            BodyShow(MotherBrainBodyFrame.HandBeam(2), 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(3), 1),
            BodyShow(MotherBrainBodyFrame.Standing, 240),
            BodyCue(MotherBrainBodyCue.AdvanceHandBeamPhase),
            Pose(MotherBrainBodyPose.Standing),
            BodyHold,
        ]);
        return [.. steps];
    }

    /// <summary>Shows a body frame.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> BodyShow(MotherBrainBodyFrame frame, int duration) =>
        AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>.Show(frame, duration);

    /// <summary>Publishes a body cue.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> BodyCue(MotherBrainBodyCue cue) =>
        AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>.Publish(cue);

    /// <summary>Publishes a body pose.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> Pose(MotherBrainBodyPose pose) =>
        BodyCue(MotherBrainBodyCue.SetPose(pose));

    /// <summary>The native sleep that ends a body program.</summary>
    private static AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue> BodyHold =>
        AnimationProgramStep<B, MotherBrainBodyFrame, MotherBrainBodyCue>.Hold;

    /// <summary>Every brain program.</summary>
    internal static AnimationProgramSet<H, MotherBrainHeadFrame, MotherBrainHeadCue> Head { get; } = new(
        new Dictionary<H, AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[]>
    {
        [H.StretchingPhase2] = [.. Stretch(2, 3, 4, MotherBrainStretchKind.Attack), FallInto(H.StretchingPhase2Loop)],
        [H.StretchingPhase2Loop] = [Show(2, 1), GoTo(H.StretchingPhase2Loop)],
        [H.StretchingPhase3] = [.. Stretch(8, 9, 10, MotherBrainStretchKind.Attack), FallInto(H.StretchingPhase3Loop)],
        [H.StretchingPhase3Loop] = [Show(8, 1), GoTo(H.StretchingPhase3Loop)],
        [H.HyperBeamRecoil] = [.. Stretch(8, 9, 10, MotherBrainStretchKind.HyperBeamRecoil), FallInto(H.HyperBeamRecoilLoop)],
        [H.HyperBeamRecoilLoop] = [Show(8, 1), GoTo(H.HyperBeamRecoilLoop)],
        [H.Initial] = [Show(0, 4), GoTo(H.Initial)],
        [H.Decapitated] = [Show(8, 8), Show(7, 4), FallInto(H.DecapitatedLoop)],
        [H.DecapitatedLoop] = [Show(6, 4), GoTo(H.DecapitatedLoop)],
        [H.DyingDrool] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(8, 4),
            Show(9, 4),
            HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.Roar)),
            FallInto(H.DyingDroolBurst),
        ],
        [H.DyingDroolBurst] =
        [
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.SpawnDrool),
            FallInto(H.DyingDroolLoop),
        ],
        [H.DyingDroolLoop] =
        [
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.MaybeDroolAgain),
            GoTo(H.DyingDroolLoop),
        ],
        [H.FiringRainbowBeam] = [Show(2, 1), GoTo(H.FiringRainbowBeam)],
        [H.NeutralPhase2] = [Show(0, 4), Show(1, 4), Show(2, 8), Show(1, 4), Show(0, 4), Show(1, 4), FallInto(H.NeutralPhase2Hold)],
        [H.NeutralPhase2Hold] =
        [
            Show(2, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase2),
            Show(1, 4),
            GoTo(H.NeutralPhase2),
        ],
        [H.NeutralPhase3] = [Show(6, 4), Show(7, 4), Show(8, 8), Show(7, 4), Show(6, 4), Show(7, 4), FallInto(H.NeutralPhase3Hold)],
        [H.NeutralPhase3Hold] =
        [
            Show(8, 8),
            Show(7, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase3),
            Show(6, 4),
            GoTo(H.NeutralPhase3),
        ],
        [H.Corpse] = [Show(6, 2), Show(7, 2), Show(8, 64), Show(11, 64), FallInto(H.CorpseLoop)],
        [H.CorpseLoop] = [Show(12, 2), GoTo(H.CorpseLoop)],
        [H.FourOnionRingsPhase2] = OnionRingsPhase2(rings: 4),
        [H.TwoOnionRingsPhase2] = OnionRingsPhase2(rings: 2),
        [H.AttackingBabyMetroid] =
        [
            Cue(MotherBrainHeadCueKind.CountBabyMetroidAttack),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid),
            GoTo(H.FourOnionRingsPhase3),
        ],
        [H.AttackingSamusFourOnionRingsPhase3] =
        [
            Cue(MotherBrainHeadCueKind.ResetBabyMetroidAttacks),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
            FallInto(H.FourOnionRingsPhase3),
        ],
        [H.FourOnionRingsPhase3] =
        [
            Show(8, 4),
            Show(9, 4),
            Cue(MotherBrainHeadCueKind.QueueBabyMetroidAttackSound),
            Show(10, 8),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            HeadCue(MotherBrainHeadCue.Sound3(MotherBrainSoundDefinitions.OnionRings)),
            Show(10, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(10, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(10, 3),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            Show(10, 16),
            Show(9, 4),
            Show(8, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(H.NeutralPhase3),
        ],
        [H.BombPhase2] = Bomb(firstHeadFrame: 0, afterburns: 7, neutral: H.NeutralPhase2),
        [H.BombPhase3] = Bomb(firstHeadFrame: 6, afterburns: 1, neutral: H.NeutralPhase3),
        [H.Laser] =
        [
            Show(1, 16),
            Show(2, 4),
            Cue(MotherBrainHeadCueKind.SpawnLaser),
            Show(2, 32),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(H.NeutralPhase2),
        ],
        [H.ChargingRainbowBeam] =
        [
            Cue(MotherBrainHeadCueKind.PrepareRainbowBeamCharge),
            Show(2, 4),
            Show(1, 4),
            Show(0, 2),
            FallInto(H.ChargingRainbowBeamLoop),
        ],
        [H.ChargingRainbowBeamLoop] =
        [
            Cue(MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle),
            Show(0, 30),
            GoTo(H.ChargingRainbowBeamLoop),
        ],
    });

    /// <summary>
    /// The neck stretch and the Hyper Beam recoil: the brain shakes, the mouth opens through
    /// <paramref name="open"/>, <paramref name="wide"/> and <paramref name="widest"/>, breathing a big purple cloud
    /// with the stretch cry, and drools while stretching.
    /// </summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[] Stretch(int open, int wide, int widest, MotherBrainStretchKind kind)
    {
        var steps = new List<AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(open, 2),
            Show(wide, 2),
        };
        if (kind == MotherBrainStretchKind.Attack) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
        steps.Add(Show(wide, 2));
        steps.Add(Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath));
        steps.Add(HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.Roar)));
        foreach (int duration in new[] { 16, 16, 32 })
        {
            if (kind == MotherBrainStretchKind.Attack) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
            steps.Add(Show(widest, duration));
        }
        steps.Add(Show(wide, 4));
        return [.. steps];
    }

    /// <summary>A phase-two onion-ring volley of <paramref name="rings"/> rings aimed at Samus.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[] OnionRingsPhase2(int rings)
    {
        var steps = new List<AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(2, 4),
            Show(3, 4),
            HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.AttackCry)),
            Show(4, 8),
        };
        for (int ring = 0; ring < rings; ring++)
        {
            steps.Add(Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus));
            steps.Add(Cue(MotherBrainHeadCueKind.SpawnOnionRing));
            if (ring == 0) steps.Add(HeadCue(MotherBrainHeadCue.Sound3(MotherBrainSoundDefinitions.OnionRings)));
            steps.Add(Show(4, ring == rings - 1 ? 16 : 3));
        }
        steps.AddRange(
        [
            Show(3, 4),
            Show(2, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(H.NeutralPhase2),
        ]);
        return [.. steps];
    }

    /// <summary>A bomb lob: phase two lobs a seven-afterburn bomb, phase three a one-afterburn bomb.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>[] Bomb(int firstHeadFrame, int afterburns, H neutral)
    {
        // Phase three uses the damaged head (frames 6 to 10) where phase two uses the whole one (0 to 4).
        int head = firstHeadFrame;
        return
        [
            Show(head, 4),
            Show(head + 1, 4),
            Show(head + 2, 8),
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(head + 2, 4),
            Show(head + 3, 4),
            HeadCue(MotherBrainHeadCue.Sound2(MotherBrainSoundDefinitions.AttackCry)),
            Show(head + 4, 8),
            HeadCue(MotherBrainHeadCue.Bomb(afterburns)),
            Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
            Show(head + 4, 32),
            Show(head + 3, 4),
            Show(head + 2, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
            GoTo(neutral),
        ];
    }

    /// <summary>Shows brain frame <paramref name="index"/> of the composition catalog.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> Show(int index, int duration) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.Show(new MotherBrainHeadFrame(index), duration);

    /// <summary>Publishes a brain cue without operands.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> Cue(MotherBrainHeadCueKind kind) =>
        HeadCue(MotherBrainHeadCue.Of(kind));

    /// <summary>Publishes a brain cue.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> HeadCue(MotherBrainHeadCue cue) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> GoTo(H program) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue> FallInto(H program) =>
        AnimationProgramStep<H, MotherBrainHeadFrame, MotherBrainHeadCue>.FallInto(program);
}
