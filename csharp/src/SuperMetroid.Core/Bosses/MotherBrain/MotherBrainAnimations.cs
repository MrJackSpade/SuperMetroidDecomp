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
        [B.HandBeam] =
        [
            Pose(MotherBrainBodyPose.DeathBeam),
            BodyShow(EnemyFrameId.MotherBrainBodyStanding, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyHandCurling, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        ],
        [B.HandBeamCharge] =
        [
            BodyCue(ChargeParticle),
            BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
            BodyCue(ChargeEnergy),
            BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
        ],
        [B.HandBeamFire] =
        [
            BodyShow(EnemyFrameId.MotherBrainBodyArmDrawnBack, 1),
            BodyCue(MotherBrainBodyCue.FireHandBeam),
            BodyShow(EnemyFrameId.MotherBrainBodyArmFlungOut, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyArmReturning, 1),
            BodyShow(EnemyFrameId.MotherBrainBodyStanding, 240),
            BodyCue(MotherBrainBodyCue.AdvanceHandBeamPhase),
            Pose(MotherBrainBodyPose.Standing),
        ],
    });

    /// <summary>How many times the hand beam charges at her elbow before it fires.</summary>
    private const int HandBeamCharges = 4;

    /// <summary>
    /// Each body animation, by the segment it starts at: every segment but the hand beam's charge and firing is played once,
    /// then held. The red hand beam raises her arm, charges (<see cref="HandBeamCharges"/>) and fires; the arm drawn
    /// back shows twice running before the beam fires, each a frame of its own on her body (#2318).
    /// </summary>
    internal static Dictionary<B, Animation<B>> BodyStartingAt { get; } = new Dictionary<B, Animation<B>>(
        Enum.GetValues<B>().Except([B.HandBeam, B.HandBeamCharge, B.HandBeamFire])
            .Select(segment => KeyValuePair.Create(segment, new Animation<B>([BodySegments.Whole(segment)], AnimationEnd.Holds))))
    {
        [B.HandBeam] = new([BodySegments.Whole(B.HandBeam), BodySegments.Repeated(B.HandBeamCharge, HandBeamCharges), BodySegments.Whole(B.HandBeamFire)],
            AnimationEnd.Holds),
    };

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
    /// Every brain segment, as it shows her brain (#2318). A picture shown twice in a row with nothing between is one
    /// frame of both: the Hyper Beam recoil opens her mouth as the stretch does without drooling, so its wide and its
    /// widest mouth each show as one frame. Her still pictures hold.
    /// </summary>
    internal static AnimationSegments<H, EnemyFrameId, MotherBrainHeadCue> HeadSegments { get; } = new(
        new Dictionary<H, AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[]>
    {
        [H.StretchingPhase2] = Stretching(WholeHead),
        [H.StretchingPhase2Held] = [Held(EnemyFrameId.MotherBrainHeadEyeGlaring)],
        [H.StretchingPhase3] = Stretching(DamagedHead),
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
        [H.DyingDroolBurst] = [Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3), Cue(MotherBrainHeadCueKind.SpawnDrool)],
        [H.DyingDroolLoop] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            Cue(MotherBrainHeadCueKind.MaybeDroolAgain),
        ],
        [H.FiringRainbowBeam] = [Held(EnemyFrameId.MotherBrainHeadEyeGlaring)],
        [H.NeutralPhase2] = Neutral(WholeHead),
        [H.NeutralPhase2Hold] =
        [
            Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 9),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase2),
            Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5),
        ],
        [H.NeutralPhase3] = Neutral(DamagedHead),
        [H.NeutralPhase3Hold] =
        [
            Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 9),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 9),
            Cue(MotherBrainHeadCueKind.MaybeHoldNeutralPhase3),
            Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 5),
        ],
        [H.Corpse] = [Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 3), Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 3), Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 65), Show(EnemyFrameId.MotherBrainHeadCorpseSlumping, 65)],
        [H.CorpseHeld] = [Held(EnemyFrameId.MotherBrainHeadCorpseSlumped)],
        [H.FourOnionRingsPhase2] = Phase2VolleyOpening,
        [H.OnionRingShotPhase2] = OnionRingShot(WholeHead, RingAimedAtSamus),
        [H.OnionRingVolleyEndPhase2] = OnionRingVolleyEnd(WholeHead),
        [H.TwoOnionRingsPhase2] = Phase2VolleyOpening,
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
        [H.FourOnionRingsPhase3] = OnionRingVolleyOpening(DamagedHead, [],
            Cue(MotherBrainHeadCueKind.QueueBabyMetroidAttackSound), RingAimedBefore),
        [H.OnionRingShotPhase3] = OnionRingShot(DamagedHead, RingAimedBefore),
        [H.OnionRingVolleyEndPhase3] = OnionRingVolleyEnd(DamagedHead),
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
    /// The brain animations, by the segment each starts at (#1948): each lists its own segments, played once in turn but
    /// for the dying drool's bursts and the onion rings each volley fires after its first (#2470), and the one it loops
    /// from, or rests on the still picture it ends at (#2353). Only
    /// the segments she starts an animation at, or a random hold or drool continues at, start one; the segments an
    /// animation only runs on into are listed in it and start none.
    /// </summary>
    internal static Dictionary<H, Animation<H>> HeadStartingAt { get; } = new Dictionary<H, Animation<H>>
    {
        [H.StretchingPhase2] = Resting(H.StretchingPhase2, H.StretchingPhase2Held),
        [H.StretchingPhase3] = Resting(H.StretchingPhase3, H.StretchingPhase3Held),
        [H.HyperBeamRecoil] = Resting(H.HyperBeamRecoil, H.HyperBeamRecoilHeld),
        [H.WholeHeadMouthClosed] = Resting(H.WholeHeadMouthClosed),
        [H.Decapitated] = Resting(H.Decapitated, H.DecapitatedHeld),
        [H.DyingDrool] = Animation<H>.LeadingInto([HeadSegments.Whole(H.DyingDrool), DroolBursts], [HeadSegments.Whole(H.DyingDroolLoop)]),
        [H.DyingDroolBurst] = Animation<H>.LeadingInto([DroolBursts], [HeadSegments.Whole(H.DyingDroolLoop)]),
        [H.FiringRainbowBeam] = Resting(H.FiringRainbowBeam),
        [H.NeutralPhase2] = Looping(H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.NeutralPhase2Hold] = Looping(H.NeutralPhase2Hold, H.NeutralPhase2),
        [H.NeutralPhase3] = Looping(H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.NeutralPhase3Hold] = Looping(H.NeutralPhase3Hold, H.NeutralPhase3),
        [H.Corpse] = Resting(H.Corpse, H.CorpseHeld),
        [H.FourOnionRingsPhase2] = OnionRingVolley([H.FourOnionRingsPhase2], H.OnionRingShotPhase2, RingsOfFour, H.OnionRingVolleyEndPhase2,
            H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.TwoOnionRingsPhase2] = OnionRingVolley([H.TwoOnionRingsPhase2], H.OnionRingShotPhase2, RingsOfTwo, H.OnionRingVolleyEndPhase2,
            H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.AttackingBabyMetroid] = OnionRingVolley([H.AttackingBabyMetroid, H.FourOnionRingsPhase3], H.OnionRingShotPhase3, RingsOfFour,
            H.OnionRingVolleyEndPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.AttackingSamusFourOnionRingsPhase3] = OnionRingVolley([H.AttackingSamusFourOnionRingsPhase3, H.FourOnionRingsPhase3], H.OnionRingShotPhase3,
            RingsOfFour, H.OnionRingVolleyEndPhase3, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.BombPhase2] = LeadingInto([H.BombPhase2], H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.BombPhase3] = LeadingInto([H.BombPhase3], H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.Laser] = LeadingInto([H.Laser], H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.ChargingRainbowBeam] = LeadingInto([H.ChargingRainbowBeam], H.ChargingRainbowBeamLoop),
    };

    /// <summary>The brain animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<H> HeadStartingWith(H segment) => HeadStartingAt[segment];

    /// <summary>How many times her dying head drools from its widest mouth in one burst.</summary>
    private const int DroolsPerBurst = 4;

    /// <summary>Her dying head's burst of drool: the widest mouth and a drool, <see cref="DroolsPerBurst"/> times over.</summary>
    private static AnimationPhase<H> DroolBursts => HeadSegments.Repeated(H.DyingDroolBurst, DroolsPerBurst);

    /// <summary>The onion rings each of her four-ring volleys fires.</summary>
    private const int RingsOfFour = 4;

    /// <summary>The onion rings her two-ring volley fires.</summary>
    private const int RingsOfTwo = 2;

    /// <summary>
    /// An onion-ring volley of <paramref name="rings"/> rings (#2470): <paramref name="opening"/>, which fires the first, then
    /// <paramref name="ringShot"/> for each ring after it and <paramref name="end"/>, each played once in turn; then each of
    /// <paramref name="cycle"/> in turn, over and over.
    /// </summary>
    private static Animation<H> OnionRingVolley(H[] opening, H ringShot, int rings, H end, params H[] cycle) =>
        Animation<H>.LeadingInto(
            [.. opening.Select(HeadSegments.Whole), HeadSegments.Repeated(ringShot, rings - 1), HeadSegments.Whole(end)],
            [.. cycle.Select(HeadSegments.Whole)]);

    /// <summary>
    /// A volley's opening from <paramref name="head"/> (#2470): after <paramref name="beforeOpening"/> its mouth opens wide
    /// with <paramref name="cry"/> and, at its widest, fires the volley's first ring, <paramref name="ring"/>, whose sound
    /// the volley plays once. Phase two's volleys first hold her neck still and cry out; phase three's held it as they
    /// aimed and cry unless she has attacked the Baby eleven times.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] OnionRingVolleyOpening(HeadMouth head,
        AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] beforeOpening, AnimationBeat<EnemyFrameId, MotherBrainHeadCue> cry,
        AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] ring) =>
    [
        .. beforeOpening,
        Show(head.Open, 5),
        Show(head.Wide, 5),
        cry,
        Show(head.Widest, 9),
        .. ring,
        HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)),
    ];

    /// <summary>One more ring of a volley from <paramref name="head"/> (#2470): its widest mouth, then <paramref name="ring"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] OnionRingShot(HeadMouth head,
        AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] ring) =>
    [
        Show(head.Widest, 4),
        .. ring,
    ];

    /// <summary>
    /// A volley's end from <paramref name="head"/> (#2470): its widest mouth holds after the last ring, closes, and lets her
    /// neck move again.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] OnionRingVolleyEnd(HeadMouth head) =>
    [
        Show(head.Widest, 17),
        Show(head.Wide, 5),
        Show(head.Open, 17),
        Cue(MotherBrainHeadCueKind.EnableNeckMovement),
    ];

    /// <summary>
    /// The opening of phase two's volleys, of four rings or two (#2470): her neck held still, her whole head cries out and
    /// fires the first ring at Samus.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] Phase2VolleyOpening =>
        OnionRingVolleyOpening(WholeHead, [Cue(MotherBrainHeadCueKind.DisableNeckMovement)],
            HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)), RingAimedAtSamus);

    /// <summary>A ring of phase two's volleys: aimed at Samus as it is fired.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] RingAimedAtSamus =>
        [Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus), Cue(MotherBrainHeadCueKind.SpawnOnionRing)];

    /// <summary>A ring of phase three's volley: fired where her head aimed the volley, at Samus or the Baby, before it opened.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] RingAimedBefore => [Cue(MotherBrainHeadCueKind.SpawnOnionRing)];

    /// <summary>
    /// A stretch of <paramref name="head"/>: it shakes, opens wide and drools, lets out a big purple breath with a roar, and
    /// drools three times more from its widest mouth before it begins to close. Phase two stretches the whole head, phase
    /// three the damaged one.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] Stretching(HeadMouth head) =>
    [
        Cue(MotherBrainHeadCueKind.ShakeBrain),
        Show(head.Open, 3),
        Show(head.Wide, 3),
        Cue(MotherBrainHeadCueKind.SpawnDrool),
        Show(head.Wide, 3),
        Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
        HeadCue(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
        Cue(MotherBrainHeadCueKind.SpawnDrool),
        Show(head.Widest, 17),
        Cue(MotherBrainHeadCueKind.SpawnDrool),
        Show(head.Widest, 17),
        Cue(MotherBrainHeadCueKind.SpawnDrool),
        Show(head.Widest, 33),
        Show(head.Wide, 5),
    ];

    /// <summary>
    /// <paramref name="head"/> idling: its mouth opens and closes again. Phase two idles with the whole head, phase three
    /// with the damaged one.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainHeadCue>[] Neutral(HeadMouth head) =>
    [
        Show(head.Closed, 5),
        Show(head.Opening, 5),
        Show(head.Open, 9),
        Show(head.Opening, 5),
        Show(head.Closed, 5),
        Show(head.Opening, 5),
    ];

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

    /// <summary>An animation that plays each of <paramref name="cycle"/> once in turn, over and over.</summary>
    private static Animation<H> Looping(params H[] cycle) => Animation<H>.Looping([.. cycle.Select(HeadSegments.Whole)]);

    /// <summary>
    /// An animation that plays each of <paramref name="lead"/> once in turn, then each of <paramref name="cycle"/> in turn,
    /// over and over (#2414).
    /// </summary>
    private static Animation<H> LeadingInto(H[] lead, params H[] cycle) =>
        Animation<H>.LeadingInto([.. lead.Select(HeadSegments.Whole)], [.. cycle.Select(HeadSegments.Whole)]);

    /// <summary>
    /// An animation of <paramref name="segments"/>, each played once in turn, that rests on the still picture the last one
    /// holds (#2353): her brain shows it for good, until she plays another.
    /// </summary>
    private static Animation<H> Resting(params H[] segments) => new([.. segments.Select(HeadSegments.Whole)], AnimationEnd.Holds);
}
