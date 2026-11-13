using SuperMetroid.Core.Programs;
using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodySegment;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadSegment;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's typed body and brain animations (#641, #1600).</summary>
internal static class MotherBrainAnimations
{
    /// <summary>Every body segment.</summary>
    internal static AnimationSegments<B, MotherBrainBodyFrame, MotherBrainBodyCue> BodySegments { get; } = new(
        new Dictionary<B, AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>[]>
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
            BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(-10, 4)),
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.Uncrouching, 8),
            BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(-16, 4)),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            Pose(MotherBrainBodyPose.Standing),
        ],
        [B.StandAfterCrouchSlow] = [.. StandUp(16)],
        [B.StandAfterCrouchFast] = [.. StandUp(8)],
        [B.StandAfterLeaning] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
            BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            Pose(MotherBrainBodyPose.Standing),
        ],
        [B.LeanDown] =
        [
            Pose(MotherBrainBodyPose.CrouchingTransition),
            BodyShow(MotherBrainBodyFrame.Standing, 8),
            BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(12, 4)),
            Pose(MotherBrainBodyPose.LeaningDown),
            BodyShow(MotherBrainBodyFrame.LeaningDown, 8),
        ],
        [B.Crouched] =
        [
            Pose(MotherBrainBodyPose.Crouched),
            BodyShow(MotherBrainBodyFrame.Crouched, 8),
        ],
        [B.CrouchSlow] = [.. Crouch(8, 8)],
        [B.CrouchFast] = [.. Crouch(2, 2)],
        [B.HandBeam] = HandBeam(),
        [B.InitialDummy] =
        [
            BodyShow(MotherBrainBodyFrame.InitialDummy, 0),
        ],
    });

    /// <summary>Each body program's animation: its segment played once, then held as the script's sleep holds it.</summary>
    internal static IReadOnlyDictionary<B, Animation<B>> BodyStartingAt { get; } =
        Enum.GetValues<B>().ToDictionary(segment => segment, segment => new Animation<B>([Whole(segment)], AnimationEnd.Holds));

    /// <summary>The body animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<B> BodyStartingWith(B segment) => BodyStartingAt[segment];

    /// <summary>A forward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>[] WalkForward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        BodyCue(new MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(0), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(2, 0))),
        BodyShow(MotherBrainBodyFrame.Walking(1), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(0, 1))),
        BodyShow(MotherBrainBodyFrame.Walking(2), duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(3, 1))),
        BodyShow(MotherBrainBodyFrame.Walking(3), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(15, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(4), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(6, -4))),
        BodyShow(MotherBrainBodyFrame.Walking(5), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-2, 4))),
        BodyShow(MotherBrainBodyFrame.Walking(6), duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(MotherBrainBodyFrame.Walking(7), duration),
    ];

    /// <summary>A backward walk whose frames last <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>[] WalkBackward(int duration) =>
    [
        Pose(MotherBrainBodyPose.Walking),
        BodyShow(MotherBrainBodyFrame.Walking(7), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(1, -2))),
        BodyShow(MotherBrainBodyFrame.Walking(6), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(2, -4))),
        BodyShow(MotherBrainBodyFrame.Walking(5), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-6, 4))),
        BodyShow(MotherBrainBodyFrame.Walking(4), duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(-15, 2))),
        BodyShow(MotherBrainBodyFrame.Walking(3), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-3, -1))),
        BodyShow(MotherBrainBodyFrame.Walking(2), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(0, -1))),
        BodyShow(MotherBrainBodyFrame.Walking(1), duration),
        BodyCue(new MotherBrainBodyCue.Move(new(-2, 0))),
        BodyShow(MotherBrainBodyFrame.Walking(0), duration),
        BodyCue(new MotherBrainBodyCue.StepAndMove(new(-1, 2))),
        Pose(MotherBrainBodyPose.Standing),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
    ];

    /// <summary>Lowers the body into the crouch; the lean and uncrouch frames last the given updates.</summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>[] Crouch(int lean, int uncrouch) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(MotherBrainBodyFrame.Standing, 8),
        BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(12, 4)),
        BodyShow(MotherBrainBodyFrame.LeaningDown, lean),
        BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(16, -2)),
        BodyShow(MotherBrainBodyFrame.Uncrouching, uncrouch),
        BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(10, -2)),
        Pose(MotherBrainBodyPose.Crouched),
        BodyShow(MotherBrainBodyFrame.Crouched, 8),
    ];

    /// <summary>Raises the body from the crouch to standing, each frame lasting <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>[] StandUp(int duration) =>
    [
        Pose(MotherBrainBodyPose.CrouchingTransition),
        BodyShow(MotherBrainBodyFrame.Crouched, duration),
        BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(-10, 4)),
        BodyShow(MotherBrainBodyFrame.Uncrouching, duration),
        BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(-16, 4)),
        BodyShow(MotherBrainBodyFrame.LeaningDown, duration),
        BodyCue(new MotherBrainBodyCue.MoveWithScrollBias(-12, -2)),
        BodyShow(MotherBrainBodyFrame.Standing, duration),
        Pose(MotherBrainBodyPose.Standing),
    ];

    /// <summary>
    /// The red hand beam: the arm rises, eight charge particles alternate between the particle and the energy
    /// spark at the hand, the beam fires, and the body holds the standing pose before the next phase.
    /// </summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>[] HandBeam()
    {
        var steps = new List<AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>>
        {
            Pose(MotherBrainBodyPose.DeathBeam),
            BodyShow(MotherBrainBodyFrame.Standing, 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(0), 1),
            BodyShow(MotherBrainBodyFrame.HandBeam(1), 1),
        };
        for (int particle = 0; particle < 8; particle++)
        {
            steps.Add(BodyCue(particle % 2 == 0
                ? new MotherBrainBodyCue.SpawnHandDust(new(36, -40), MiscDustAnimation.MotherBrainElbowChargeParticle)
                : new MotherBrainBodyCue.SpawnHandDust(new(34, -42), MiscDustAnimation.MotherBrainElbowChargeEnergy)));
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
        ]);
        return [.. steps];
    }

    /// <summary>Shows a body frame.</summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue> BodyShow(MotherBrainBodyFrame frame, int duration) =>
        new AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>.Frame(frame, duration);

    /// <summary>Raises a body event.</summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue> BodyCue(MotherBrainBodyCue cue) =>
        new AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue>.Event(cue);

    /// <summary>Raises a body pose.</summary>
    private static AnimationBeat<MotherBrainBodyFrame, MotherBrainBodyCue> Pose(MotherBrainBodyPose pose) =>
        BodyCue(new MotherBrainBodyCue.SetPose(pose));

    /// <summary>Every brain segment.</summary>
    internal static AnimationSegments<H, MotherBrainHeadFrame, MotherBrainHeadCue> HeadSegments { get; } = new(
        new Dictionary<H, AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>[]>
    {
        [H.StretchingPhase2] = [.. Stretch(2, 3, 4, MotherBrainStretchKind.Attack)],
        [H.StretchingPhase2Loop] = [Show(2, 1)],
        [H.StretchingPhase3] = [.. Stretch(8, 9, 10, MotherBrainStretchKind.Attack)],
        [H.StretchingPhase3Loop] = [Show(8, 1)],
        [H.HyperBeamRecoil] = [.. Stretch(8, 9, 10, MotherBrainStretchKind.HyperBeamRecoil)],
        [H.HyperBeamRecoilLoop] = [Show(8, 1)],
        [H.Initial] = [Show(0, 4)],
        [H.Decapitated] = [Show(8, 8), Show(7, 4)],
        [H.DecapitatedLoop] = [Show(6, 4)],
        [H.DyingDrool] =
        [
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(8, 4),
            Show(9, 4),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
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
        ],
        [H.DyingDroolLoop] =
        [
            Show(10, 2),
            Cue(MotherBrainHeadCueKind.MaybeDroolAgain),
        ],
        [H.FiringRainbowBeam] = [Show(2, 1)],
        [H.NeutralPhase2] = [Show(0, 4), Show(1, 4), Show(2, 8), Show(1, 4), Show(0, 4), Show(1, 4)],
        [H.NeutralPhase2Hold] =
        [
            Show(2, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase2),
            Show(1, 4),
        ],
        [H.NeutralPhase3] = [Show(6, 4), Show(7, 4), Show(8, 8), Show(7, 4), Show(6, 4), Show(7, 4)],
        [H.NeutralPhase3Hold] =
        [
            Show(8, 8),
            Show(7, 8),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase3),
            Show(6, 4),
        ],
        [H.Corpse] = [Show(6, 2), Show(7, 2), Show(8, 64), Show(11, 64)],
        [H.CorpseLoop] = [Show(12, 2)],
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
            Show(8, 4),
            Show(9, 4),
            Cue(MotherBrainHeadCueKind.QueueBabyMetroidAttackSound),
            Show(10, 8),
            Cue(MotherBrainHeadCueKind.SpawnOnionRing),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)),
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
        ],
        [H.BombPhase2] = Bomb(firstHeadFrame: 0, afterburns: 7),
        [H.BombPhase3] = Bomb(firstHeadFrame: 6, afterburns: 1),
        [H.Laser] =
        [
            Show(1, 16),
            Show(2, 4),
            Cue(MotherBrainHeadCueKind.SpawnLaser),
            Show(2, 32),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.ChargingRainbowBeam] =
        [
            Cue(MotherBrainHeadCueKind.PrepareRainbowBeamCharge),
            Show(2, 4),
            Show(1, 4),
            Show(0, 2),
        ],
        [H.ChargingRainbowBeamLoop] =
        [
            Cue(MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle),
            Show(0, 30),
        ],
    });

    /// <summary>
    /// The brain animation that starts at each brain program: each plays on through the programs its list runs on into or
    /// continues at, looping from the first one it reaches again.
    /// </summary>
    internal static IReadOnlyDictionary<H, Animation<H>> HeadStartingAt { get; } = new Dictionary<H, Animation<H>>
    {
        [H.StretchingPhase2] = Looping(1, H.StretchingPhase2, H.StretchingPhase2Loop),
        [H.StretchingPhase2Loop] = Looping(0, H.StretchingPhase2Loop),
        [H.StretchingPhase3] = Looping(1, H.StretchingPhase3, H.StretchingPhase3Loop),
        [H.StretchingPhase3Loop] = Looping(0, H.StretchingPhase3Loop),
        [H.HyperBeamRecoil] = Looping(1, H.HyperBeamRecoil, H.HyperBeamRecoilLoop),
        [H.HyperBeamRecoilLoop] = Looping(0, H.HyperBeamRecoilLoop),
        [H.Initial] = Looping(0, H.Initial),
        [H.Decapitated] = Looping(1, H.Decapitated, H.DecapitatedLoop),
        [H.DecapitatedLoop] = Looping(0, H.DecapitatedLoop),
        [H.DyingDrool] = Looping(2, H.DyingDrool, H.DyingDroolBurst, H.DyingDroolLoop),
        [H.DyingDroolBurst] = Looping(1, H.DyingDroolBurst, H.DyingDroolLoop),
        [H.DyingDroolLoop] = Looping(0, H.DyingDroolLoop),
        [H.FiringRainbowBeam] = Looping(0, H.FiringRainbowBeam),
        [H.NeutralPhase2] = Looping(0, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.NeutralPhase2Hold] = Looping(0, H.NeutralPhase2Hold, H.NeutralPhase2),
        [H.NeutralPhase3] = Looping(0, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.NeutralPhase3Hold] = Looping(0, H.NeutralPhase3Hold, H.NeutralPhase3),
        [H.Corpse] = Looping(1, H.Corpse, H.CorpseLoop),
        [H.CorpseLoop] = Looping(0, H.CorpseLoop),
        [H.FourOnionRingsPhase2] = Looping(1, H.FourOnionRingsPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.TwoOnionRingsPhase2] = Looping(1, H.TwoOnionRingsPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.AttackingBabyMetroid] = Looping(2, H.AttackingBabyMetroid, H.FourOnionRingsPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.AttackingSamusFourOnionRingsPhase3] = Looping(2, H.AttackingSamusFourOnionRingsPhase3, H.FourOnionRingsPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.FourOnionRingsPhase3] = Looping(1, H.FourOnionRingsPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.BombPhase2] = Looping(1, H.BombPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.BombPhase3] = Looping(1, H.BombPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.Laser] = Looping(1, H.Laser, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.ChargingRainbowBeam] = Looping(1, H.ChargingRainbowBeam, H.ChargingRainbowBeamLoop),
        [H.ChargingRainbowBeamLoop] = Looping(0, H.ChargingRainbowBeamLoop),
    };

    /// <summary>The brain animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<H> HeadStartingWith(H segment) => HeadStartingAt[segment];

    /// <summary>
    /// The neck stretch and the Hyper Beam recoil: the brain shakes, the mouth opens through
    /// <paramref name="open"/>, <paramref name="wide"/> and <paramref name="widest"/>, breathing a big purple cloud
    /// with the stretch cry, and drools while stretching.
    /// </summary>
    private static AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>[] Stretch(int open, int wide, int widest, MotherBrainStretchKind kind)
    {
        var steps = new List<AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.ShakeBrain),
            Show(open, 2),
            Show(wide, 2),
        };
        if (kind == MotherBrainStretchKind.Attack) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
        steps.Add(Show(wide, 2));
        steps.Add(Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath));
        steps.Add(HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)));
        foreach (int duration in new[] { 16, 16, 32 })
        {
            if (kind == MotherBrainStretchKind.Attack) steps.Add(Cue(MotherBrainHeadCueKind.SpawnDrool));
            steps.Add(Show(widest, duration));
        }
        steps.Add(Show(wide, 4));
        return [.. steps];
    }

    /// <summary>A phase-two onion-ring volley of <paramref name="rings"/> rings aimed at Samus.</summary>
    private static AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>[] OnionRingsPhase2(int rings)
    {
        var steps = new List<AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>>
        {
            Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            Show(2, 4),
            Show(3, 4),
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
            Show(4, 8),
        };
        for (int ring = 0; ring < rings; ring++)
        {
            steps.Add(Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus));
            steps.Add(Cue(MotherBrainHeadCueKind.SpawnOnionRing));
            if (ring == 0) steps.Add(HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)));
            steps.Add(Show(4, ring == rings - 1 ? 16 : 3));
        }
        steps.AddRange(
        [
            Show(3, 4),
            Show(2, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ]);
        return [.. steps];
    }

    /// <summary>A bomb lob: phase two lobs a seven-afterburn bomb, phase three a one-afterburn bomb.</summary>
    private static AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>[] Bomb(int firstHeadFrame, int afterburns)
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
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
            Show(head + 4, 8),
            HeadCue(MotherBrainHeadCue.Bomb(afterburns)),
            Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
            Show(head + 4, 32),
            Show(head + 3, 4),
            Show(head + 2, 16),
            Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ];
    }

    /// <summary>Shows brain frame <paramref name="index"/> of the composition catalog.</summary>
    private static AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue> Show(int index, int duration) =>
        new AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>.Frame(new MotherBrainHeadFrame(index), duration);

    /// <summary>Raises a brain event without operands.</summary>
    private static AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue> Cue(MotherBrainHeadCueKind kind) =>
        HeadCue(MotherBrainHeadCue.Of(kind));

    /// <summary>Raises a brain event.</summary>
    private static AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue> HeadCue(MotherBrainHeadCue cue) =>
        new AnimationBeat<MotherBrainHeadFrame, MotherBrainHeadCue>.Event(cue);

    /// <summary>One play of every beat of body segment <paramref name="segment"/>.</summary>
    private static AnimationPhase<B> Whole(B segment) => new(segment, 0, BodySegments.Beats(segment).Count, 1);

    /// <summary>One play of every beat of brain segment <paramref name="segment"/>.</summary>
    private static AnimationPhase<H> Whole(H segment) => new(segment, 0, HeadSegments.Beats(segment).Count, 1);

    /// <summary>An animation of <paramref name="segments"/>, each played once in turn, that loops from phase <paramref name="loopsFrom"/>.</summary>
    private static Animation<H> Looping(int loopsFrom, params H[] segments) => new([.. segments.Select(segment => Whole(segment))], AnimationEnd.LoopsFrom(loopsFrom));
}
