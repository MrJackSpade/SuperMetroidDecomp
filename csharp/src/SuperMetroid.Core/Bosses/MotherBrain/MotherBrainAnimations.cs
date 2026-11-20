using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodySegment;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadSegment;
using L = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainLateFightHeadSegment;
using A = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBabyAttackSegment;
using HeadBeats = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBrainBeats<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadChanceCueKind>;
using LateFightBeats = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBrainBeats<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainLateFightCueKind>;
using BabyAttackBeats = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBrainBeats<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBabyAttackCueKind>;
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
    /// then held. The red hand beam raises her arm, charges four times and fires; the arm drawn back shows twice running
    /// before the beam fires: her body shows each new frame of its own on her body layer, so the second showing is a frame
    /// of its own (#2318).
    /// </summary>
    internal static IReadOnlyDictionary<B, Animation<B>> BodyStartingAt { get; } = new Dictionary<B, Animation<B>>(
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
    /// Every segment of her head's own brain animations, as its list shows it (#2318). Nothing waits on her brain showing a
    /// new frame, so a picture its list shows twice running with nothing between is one frame of both: the Hyper Beam recoil
    /// opens her mouth as the stretch does without drooling, so its wide and its widest mouth each show as one frame. Her
    /// still pictures hold.
    /// </summary>
    internal static AnimationSegments<H, EnemyFrameId, MotherBrainBrainCue<MotherBrainHeadChanceCueKind>> HeadSegments { get; } = new(
        new Dictionary<H, AnimationBeat<EnemyFrameId, MotherBrainBrainCue<MotherBrainHeadChanceCueKind>>[]>
    {
        [H.StretchingPhase2] = Stretching(MotherBrainHeadMouth.Whole),
        [H.StretchingPhase2Held] = [HeadBeats.Held(EnemyFrameId.MotherBrainHeadEyeGlaring)],
        [H.StretchingPhase3] = Stretching(MotherBrainHeadMouth.Damaged),
        [H.StretchingPhase3Held] = [HeadBeats.Held(EnemyFrameId.MotherBrainDarkHeadEyeGlaring)],
        [H.HyperBeamRecoil] =
        [
            HeadBeats.Cue(MotherBrainHeadCueKind.ShakeBrain),
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 3),
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 6),
            HeadBeats.Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
            HeadBeats.Effect(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 67),
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 5),
        ],
        [H.HyperBeamRecoilHeld] = [HeadBeats.Held(EnemyFrameId.MotherBrainDarkHeadEyeGlaring)],
        [H.WholeHeadMouthClosed] = [HeadBeats.Held(EnemyFrameId.MotherBrainHeadEyeShut)],
        [H.Decapitated] = [HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 9), HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 5)],
        [H.DecapitatedHeld] = [HeadBeats.Held(EnemyFrameId.MotherBrainDarkHeadEyeShut)],
        [H.DyingDrool] =
        [
            HeadBeats.Cue(MotherBrainHeadCueKind.ShakeBrain),
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 5),
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadJawDropping, 5),
            HeadBeats.Effect(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
        ],
        [H.DyingDroolBurst] = [HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3), HeadBeats.Cue(MotherBrainHeadCueKind.SpawnDrool)],
        [H.DyingDroolLoop] =
        [
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadJawDropped, 3),
            HeadBeats.Own(MotherBrainHeadChanceCueKind.MaybeDroolAgain),
        ],
        [H.FiringRainbowBeam] = [HeadBeats.Held(EnemyFrameId.MotherBrainHeadEyeGlaring)],
        [H.NeutralPhase2] = HeadBeats.Neutral(MotherBrainHeadMouth.Whole),
        [H.NeutralPhase2Hold] =
        [
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 9),
            HeadBeats.Own(MotherBrainHeadChanceCueKind.MaybeHoldNeutralPhase2),
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5),
        ],
        [H.NeutralPhase3] = HeadBeats.Neutral(MotherBrainHeadMouth.Damaged),
        [H.NeutralPhase3Hold] = HeadBeats.NeutralPhase3Hold(MotherBrainHeadChanceCueKind.MaybeHoldNeutralPhase3),
        [H.Corpse] =
        [
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 3), HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 3),
            HeadBeats.Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 65), HeadBeats.Show(EnemyFrameId.MotherBrainHeadCorpseSlumping, 65),
        ],
        [H.CorpseHeld] = [HeadBeats.Held(EnemyFrameId.MotherBrainHeadCorpseSlumped)],
        [H.FourOnionRingsPhase2] = Phase2VolleyOpening,
        [H.OnionRingShotPhase2] = HeadBeats.OnionRingShot(MotherBrainHeadMouth.Whole, RingAimedAtSamus),
        [H.OnionRingVolleyEndPhase2] = HeadBeats.OnionRingVolleyEnd(MotherBrainHeadMouth.Whole),
        [H.TwoOnionRingsPhase2] = Phase2VolleyOpening,
        [H.BombPhase2] = Bomb(MotherBrainHeadMouth.Whole, afterburns: 7),
        [H.BombPhase3] = Bomb(MotherBrainHeadMouth.Damaged, afterburns: 1),
        [H.Laser] =
        [
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeOpen, 17),
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 5),
            HeadBeats.Cue(MotherBrainHeadCueKind.SpawnLaser),
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 33),
            HeadBeats.Cue(MotherBrainHeadCueKind.EnableNeckMovement),
        ],
        [H.ChargingRainbowBeam] =
        [
            HeadBeats.Cue(MotherBrainHeadCueKind.PrepareRainbowBeamCharge),
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeGlaring, 5),
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeOpen, 5),
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeShut, 3),
        ],
        [H.ChargingRainbowBeamLoop] =
        [
            HeadBeats.Cue(MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle),
            HeadBeats.Show(EnemyFrameId.MotherBrainHeadEyeShut, 31),
        ],
    });

    /// <summary>
    /// Her head's own brain animations, by the segment each starts at (#1948): each lists its own segments, played once in
    /// turn but for the dying drool's four bursts and the onion rings each volley fires after its first (#2470), and the one
    /// it loops from, or rests on the still picture it ends at (#2353). Only the segments she starts an animation at, or a
    /// random hold or drool continues at, start one; the segments an animation only runs on into are listed in it and start
    /// none.
    /// </summary>
    internal static IReadOnlyDictionary<H, Animation<H>> HeadStartingAt { get; } = new Dictionary<H, Animation<H>>
    {
        [H.StretchingPhase2] = Resting(HeadSegments, H.StretchingPhase2, H.StretchingPhase2Held),
        [H.StretchingPhase3] = Resting(HeadSegments, H.StretchingPhase3, H.StretchingPhase3Held),
        [H.HyperBeamRecoil] = Resting(HeadSegments, H.HyperBeamRecoil, H.HyperBeamRecoilHeld),
        [H.WholeHeadMouthClosed] = Resting(HeadSegments, H.WholeHeadMouthClosed),
        [H.Decapitated] = Resting(HeadSegments, H.Decapitated, H.DecapitatedHeld),
        [H.DyingDrool] = Animation<H>.LeadingInto([HeadSegments.Whole(H.DyingDrool), DroolBursts], [HeadSegments.Whole(H.DyingDroolLoop)]),
        [H.DyingDroolBurst] = Animation<H>.LeadingInto([DroolBursts], [HeadSegments.Whole(H.DyingDroolLoop)]),
        [H.FiringRainbowBeam] = Resting(HeadSegments, H.FiringRainbowBeam),
        [H.NeutralPhase2] = Looping(HeadSegments, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.NeutralPhase2Hold] = Looping(HeadSegments, H.NeutralPhase2Hold, H.NeutralPhase2),
        [H.NeutralPhase3] = Looping(HeadSegments, H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.NeutralPhase3Hold] = Looping(HeadSegments, H.NeutralPhase3Hold, H.NeutralPhase3),
        [H.Corpse] = Resting(HeadSegments, H.Corpse, H.CorpseHeld),
        [H.FourOnionRingsPhase2] = OnionRingVolley(HeadSegments, [H.FourOnionRingsPhase2], H.OnionRingShotPhase2, RingsOfFour,
            H.OnionRingVolleyEndPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.TwoOnionRingsPhase2] = OnionRingVolley(HeadSegments, [H.TwoOnionRingsPhase2], H.OnionRingShotPhase2, RingsOfTwo,
            H.OnionRingVolleyEndPhase2, H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.BombPhase2] = LeadingInto(HeadSegments, [H.BombPhase2], H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.BombPhase3] = LeadingInto(HeadSegments, [H.BombPhase3], H.NeutralPhase3, H.NeutralPhase3Hold),
        [H.Laser] = LeadingInto(HeadSegments, [H.Laser], H.NeutralPhase2, H.NeutralPhase2Hold),
        [H.ChargingRainbowBeam] = LeadingInto(HeadSegments, [H.ChargingRainbowBeam], H.ChargingRainbowBeamLoop),
    };

    /// <summary>Her head's own brain animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<H> HeadStartingWith(H segment) => HeadStartingAt[segment];

    /// <summary>
    /// Every segment of her late fight's four-ring volley at Samus (#2554): her damaged head clears its count of attacks on
    /// the Baby and aims at Samus, then fires the volley and idles.
    /// </summary>
    internal static AnimationSegments<L, EnemyFrameId, MotherBrainBrainCue<MotherBrainLateFightCueKind>> LateFightSegments { get; } = new(
        LateFightBeats.Phase3Volley(L.AttackingSamusFourOnionRingsPhase3,
        [
            LateFightBeats.Own(MotherBrainLateFightCueKind.ResetBabyMetroidAttacks),
            LateFightBeats.Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            LateFightBeats.Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus),
        ],
        new MotherBrainVolleySegments<L>(L.FourOnionRingsPhase3, L.OnionRingShotPhase3, L.OnionRingVolleyEndPhase3, L.NeutralPhase3, L.NeutralPhase3Hold),
        MotherBrainLateFightCueKind.QueueBabyMetroidAttackSound, MotherBrainLateFightCueKind.MaybeHoldNeutralPhase3));

    /// <summary>
    /// Her late fight's volley at Samus, by the segment each of its animations starts at (#2554): the volley, which then
    /// idles over and over, and the idle's hold, which a random hold continues at.
    /// </summary>
    internal static IReadOnlyDictionary<L, Animation<L>> LateFightStartingAt { get; } = new Dictionary<L, Animation<L>>
    {
        [L.AttackingSamusFourOnionRingsPhase3] = OnionRingVolley(LateFightSegments, [L.AttackingSamusFourOnionRingsPhase3, L.FourOnionRingsPhase3],
            L.OnionRingShotPhase3, RingsOfFour, L.OnionRingVolleyEndPhase3, L.NeutralPhase3, L.NeutralPhase3Hold),
        [L.NeutralPhase3Hold] = Looping(LateFightSegments, L.NeutralPhase3Hold, L.NeutralPhase3),
    };

    /// <summary>The animation of her late fight's volley at Samus that starts at <paramref name="segment"/>.</summary>
    internal static Animation<L> LateFightStartingWith(L segment) => LateFightStartingAt[segment];

    /// <summary>
    /// Every segment of her volley at the arrived Baby (#2554): her damaged head counts an attack on the Baby and aims at it,
    /// then fires the volley and idles.
    /// </summary>
    internal static AnimationSegments<A, EnemyFrameId, MotherBrainBrainCue<MotherBrainBabyAttackCueKind>> BabyAttackSegments { get; } = new(
        BabyAttackBeats.Phase3Volley(A.AttackingBabyMetroid,
        [
            BabyAttackBeats.Own(MotherBrainBabyAttackCueKind.CountBabyMetroidAttack),
            BabyAttackBeats.Cue(MotherBrainHeadCueKind.DisableNeckMovement),
            BabyAttackBeats.Own(MotherBrainBabyAttackCueKind.AimOnionRingsAtBabyMetroid),
        ],
        new MotherBrainVolleySegments<A>(A.FourOnionRingsPhase3, A.OnionRingShotPhase3, A.OnionRingVolleyEndPhase3, A.NeutralPhase3, A.NeutralPhase3Hold),
        MotherBrainBabyAttackCueKind.QueueBabyMetroidAttackSound, MotherBrainBabyAttackCueKind.MaybeHoldNeutralPhase3));

    /// <summary>
    /// Her volley at the arrived Baby, by the segment each of its animations starts at (#2554): the volley, which then idles
    /// over and over, and the idle's hold, which a random hold continues at.
    /// </summary>
    internal static IReadOnlyDictionary<A, Animation<A>> BabyAttackStartingAt { get; } = new Dictionary<A, Animation<A>>
    {
        [A.AttackingBabyMetroid] = OnionRingVolley(BabyAttackSegments, [A.AttackingBabyMetroid, A.FourOnionRingsPhase3],
            A.OnionRingShotPhase3, RingsOfFour, A.OnionRingVolleyEndPhase3, A.NeutralPhase3, A.NeutralPhase3Hold),
        [A.NeutralPhase3Hold] = Looping(BabyAttackSegments, A.NeutralPhase3Hold, A.NeutralPhase3),
    };

    /// <summary>The animation of her volley at the arrived Baby that starts at <paramref name="segment"/>.</summary>
    internal static Animation<A> BabyAttackStartingWith(A segment) => BabyAttackStartingAt[segment];

    /// <summary>How many times her dying head drools from its widest mouth in one burst.</summary>
    private const int DroolsPerBurst = 4;

    /// <summary>Her dying head's burst of drool: the widest mouth and a drool, four times.</summary>
    private static AnimationPhase<H> DroolBursts => HeadSegments.Repeated(H.DyingDroolBurst, DroolsPerBurst);

    /// <summary>The onion rings each of her four-ring volleys fires.</summary>
    private const int RingsOfFour = 4;

    /// <summary>The onion rings her two-ring volley fires.</summary>
    private const int RingsOfTwo = 2;

    /// <summary>
    /// An onion-ring volley of <paramref name="rings"/> rings (#2470) over <paramref name="segments"/>:
    /// <paramref name="opening"/>, which fires the first, then <paramref name="ringShot"/> for each ring after it and
    /// <paramref name="end"/>, each played once in turn; then each of <paramref name="cycle"/> in turn, over and over.
    /// </summary>
    private static Animation<TSegment> OnionRingVolley<TSegment, TCue>(AnimationSegments<TSegment, EnemyFrameId, TCue> segments,
        TSegment[] opening, TSegment ringShot, int rings, TSegment end, params TSegment[] cycle)
        where TSegment : struct
        where TCue : notnull =>
        Animation<TSegment>.LeadingInto(
            [.. opening.Select(segments.Whole), segments.Repeated(ringShot, rings - 1), segments.Whole(end)],
            [.. cycle.Select(segments.Whole)]);

    /// <summary>
    /// The opening of phase two's volleys, of four rings or two (#2470): her neck held still, her whole head cries out and
    /// fires the first ring at Samus.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<MotherBrainHeadChanceCueKind>>[] Phase2VolleyOpening =>
        HeadBeats.OnionRingVolleyOpening(MotherBrainHeadMouth.Whole, [HeadBeats.Cue(MotherBrainHeadCueKind.DisableNeckMovement)],
            HeadBeats.Effect(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)), RingAimedAtSamus);

    /// <summary>A ring of phase two's volleys: aimed at Samus as it is fired.</summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<MotherBrainHeadChanceCueKind>>[] RingAimedAtSamus =>
        [HeadBeats.Cue(MotherBrainHeadCueKind.AimOnionRingsAtSamus), HeadBeats.Cue(MotherBrainHeadCueKind.SpawnOnionRing)];

    /// <summary>
    /// A stretch of <paramref name="head"/>: it shakes, opens wide and drools, lets out a big purple breath with a roar, and
    /// drools three times more from its widest mouth before it begins to close. Phase two stretches the whole head, phase
    /// three the damaged one.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<MotherBrainHeadChanceCueKind>>[] Stretching(MotherBrainHeadMouth head) =>
    [
        HeadBeats.Cue(MotherBrainHeadCueKind.ShakeBrain),
        HeadBeats.Show(head.Open, 3),
        HeadBeats.Show(head.Wide, 3),
        HeadBeats.Cue(MotherBrainHeadCueKind.SpawnDrool),
        HeadBeats.Show(head.Wide, 3),
        HeadBeats.Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
        HeadBeats.Effect(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.Roar)),
        HeadBeats.Cue(MotherBrainHeadCueKind.SpawnDrool),
        HeadBeats.Show(head.Widest, 17),
        HeadBeats.Cue(MotherBrainHeadCueKind.SpawnDrool),
        HeadBeats.Show(head.Widest, 17),
        HeadBeats.Cue(MotherBrainHeadCueKind.SpawnDrool),
        HeadBeats.Show(head.Widest, 33),
        HeadBeats.Show(head.Wide, 5),
    ];

    /// <summary>
    /// A bomb lob from <paramref name="head"/>: phase two lobs a seven-afterburn bomb from the whole head, phase three a
    /// one-afterburn bomb from the damaged one.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<MotherBrainHeadChanceCueKind>>[] Bomb(MotherBrainHeadMouth head, int afterburns) =>
    [
        HeadBeats.Show(head.Closed, 5),
        HeadBeats.Show(head.Opening, 5),
        HeadBeats.Show(head.Open, 9),
        HeadBeats.Cue(MotherBrainHeadCueKind.DisableNeckMovement),
        HeadBeats.Show(head.Open, 5),
        HeadBeats.Show(head.Wide, 5),
        HeadBeats.Effect(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.AttackCry)),
        HeadBeats.Show(head.Widest, 9),
        HeadBeats.Effect(MotherBrainHeadCue.Bomb(afterburns)),
        HeadBeats.Cue(MotherBrainHeadCueKind.SpawnBigPurpleBreath),
        HeadBeats.Show(head.Widest, 33),
        HeadBeats.Show(head.Wide, 5),
        HeadBeats.Show(head.Open, 17),
        HeadBeats.Cue(MotherBrainHeadCueKind.EnableNeckMovement),
    ];

    /// <summary>An animation of <paramref name="segments"/> that plays each of <paramref name="cycle"/> once in turn, over and over.</summary>
    private static Animation<TSegment> Looping<TSegment, TCue>(AnimationSegments<TSegment, EnemyFrameId, TCue> segments, params TSegment[] cycle)
        where TSegment : struct
        where TCue : notnull =>
        Animation<TSegment>.Looping([.. cycle.Select(segments.Whole)]);

    /// <summary>
    /// An animation of <paramref name="segments"/> that plays each of <paramref name="lead"/> once in turn, then each of
    /// <paramref name="cycle"/> in turn, over and over (#2414).
    /// </summary>
    private static Animation<TSegment> LeadingInto<TSegment, TCue>(AnimationSegments<TSegment, EnemyFrameId, TCue> segments, TSegment[] lead,
        params TSegment[] cycle)
        where TSegment : struct
        where TCue : notnull =>
        Animation<TSegment>.LeadingInto([.. lead.Select(segments.Whole)], [.. cycle.Select(segments.Whole)]);

    /// <summary>
    /// An animation of <paramref name="played"/> from <paramref name="segments"/>, each played once in turn, that rests on the
    /// still picture the last one holds (#2353): her brain shows it for good, until she plays another.
    /// </summary>
    private static Animation<TSegment> Resting<TSegment, TCue>(AnimationSegments<TSegment, EnemyFrameId, TCue> segments, params TSegment[] played)
        where TSegment : struct
        where TCue : notnull =>
        new([.. played.Select(segments.Whole)], AnimationEnd.Holds);
}
