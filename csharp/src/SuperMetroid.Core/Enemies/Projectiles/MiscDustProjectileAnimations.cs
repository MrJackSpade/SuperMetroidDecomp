using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Samus.Projectiles.MiscDustProjectileSegment;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The thirty misc-dust explosion, smoke, dust, beam and drop animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class MiscDustProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MiscDustProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<MiscDustProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.BeamCharge] =
        [
            Show(EnemyProjectileFrameId.MiscDustBeamChargeFirst, 3),
            Show(EnemyProjectileFrameId.MiscDustBeamChargeSecond, 3),
            Show(EnemyProjectileFrameId.MiscDustBeamChargeThird, 3),
            Show(EnemyProjectileFrameId.MiscDustBeamChargeFourth, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ElbowChargeParticle] =
        [
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleSecond, 4),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleThird, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFourth, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFifth, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleSixth, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ElbowChargeEnergy] =
        [
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFirst, 4),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergySecond, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyThird, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFourth, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFifth, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergySixth, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergySeventh, 12),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFirst, 4),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionSecond, 6),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionThird, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFifth, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionSixth, 6),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.RainbowBeamExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFirst, 3),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionSecond, 3),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionThird, 4),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFourth, 4),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFifth, 4),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.BeamTrail] =
        [
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFirst, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailSecond, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailThird, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFourth, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFifth, 24),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.DudShot] =
        [
            Show(EnemyProjectileFrameId.MiscDustDudShotFirst, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotSecond, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotThird, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFourth, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFifth, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotSixth, 4),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.PowerBomb] =
        [
            Show(EnemyProjectileFrameId.MiscDustPowerBombFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustPowerBombSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustPowerBombThird, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ElevatorPad] =
        [
            Show(EnemyProjectileFrameId.MiscDustElevatorPadFirst, 1),
            Show(EnemyProjectileFrameId.MiscDustElevatorPadSecond, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SmallDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudThird, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFifth, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.CorpseDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFirst, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudSecond, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudThird, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFourth, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.EyeDoorSweatDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropThird, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFourth, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.Smoke] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmokeFirst, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeSecond, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeThird, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeFourth, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.LoopingElevatorPad] =
        [
            Show(EnemyProjectileFrameId.MiscDustLoopingElevatorPadFirst, 1),
            Show(EnemyProjectileFrameId.MiscDustLoopingElevatorPadSecond, 1),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionThird, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFifth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionSixth, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SmallEnergyDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropFirst, 8),
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropSecond, 8),
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropThird, 8),
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropFourth, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.BigEnergyDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropFirst, 8),
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropSecond, 8),
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropThird, 8),
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropFourth, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.Bomb] =
        [
            Show(EnemyProjectileFrameId.MiscDustBombFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustBombSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustBombThird, 5),
            Show(EnemyProjectileFrameId.MiscDustBombFourth, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.WeirdEnergyDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustWeirdEnergyDropFirst, 16),
            Show(EnemyProjectileFrameId.MiscDustWeirdEnergyDropSecond, 16),
            Show(EnemyProjectileFrameId.MiscDustWeirdEnergyDropThird, 16),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.RockParticles] =
        [
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFirst, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSecond, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesThird, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFourth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFifth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSixth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSeventh, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesEighth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesNinth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesTenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesEleventh, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesTwelfth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesThirteenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFourteenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFifteenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSixteenth, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFirst, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudSecond, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudThird, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFourth, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ShortBeamCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFirst, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudSecond, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudThird, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFourth, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFifth, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudSixth, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.MediumBeamCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFirst, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudSecond, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudThird, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFourth, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFifth, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudSixth, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudSeventh, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudThird, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFifth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudSixth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudSeventh, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudEighth, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.LongBeam] =
        [
            Show(EnemyProjectileFrameId.MiscDustLongBeamFirst, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamSecond, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamThird, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFourth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFifth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamSixth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamSeventh, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamEighth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamNinth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamEleventh, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwelfth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamThirteenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFourteenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFifteenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamSixteenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamSeventeenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamEighteenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamNineteenth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentieth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentyFirst, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentySecond, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentyThird, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentyFourth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentyFifth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentySixth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentySeventh, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentyEighth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamTwentyNinth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamThirtieth, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamThirtyFirst, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.FlickeringLongBeam] =
        [
            Show(EnemyProjectileFrameId.MiscDustFlickeringLongBeamFirst, 1),
            Show(EnemyProjectileFrameId.MiscDustFlickeringLongBeamSecond, 1),
            Show(EnemyProjectileFrameId.MiscDustFlickeringLongBeamThird, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.DraygonBreathBubbles] =
        [
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFirst, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesSecond, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesThird, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFourth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFifth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesSixth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesSeventh, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesEighth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesNinth, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SaveStationElectricity] =
        [
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFirst, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricitySecond, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityThird, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFourth, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFifth, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricitySixth, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricitySeventh, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityEighth, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ExpandingGate] =
        [
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFirst, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateSecond, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateThird, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFourth, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFifth, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateSixth, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateSeventh, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateEighth, 16),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ContractingGate] =
        [
            Show(EnemyProjectileFrameId.MiscDustContractingGateFirst, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateSecond, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateThird, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFourth, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFifth, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateSixth, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateSeventh, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateEighth, 4),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MiscDustProjectileSegment, Animation<MiscDustProjectileSegment>> StartingAt { get; } = new Dictionary<MiscDustProjectileSegment, Animation<MiscDustProjectileSegment>>
    {
        [P.BeamCharge] = new([new(P.BeamCharge, 0, 5, 1)], AnimationEnd.Holds),
        [P.ElbowChargeParticle] = new([new(P.ElbowChargeParticle, 0, 7, 1)], AnimationEnd.Holds),
        [P.ElbowChargeEnergy] = new([new(P.ElbowChargeEnergy, 0, 8, 1)], AnimationEnd.Holds),
        [P.SmallExplosion] = new([new(P.SmallExplosion, 0, 7, 1)], AnimationEnd.Holds),
        [P.RainbowBeamExplosion] = new([new(P.RainbowBeamExplosion, 0, 6, 1)], AnimationEnd.Holds),
        [P.BeamTrail] = new([new(P.BeamTrail, 0, 6, 1)], AnimationEnd.Holds),
        [P.DudShot] = new([new(P.DudShot, 0, 7, 1)], AnimationEnd.Holds),
        [P.PowerBomb] = new([new(P.PowerBomb, 0, 4, 1)], AnimationEnd.Holds),
        [P.ElevatorPad] = new([new(P.ElevatorPad, 0, 3, 1)], AnimationEnd.Holds),
        [P.SmallDustCloud] = new([new(P.SmallDustCloud, 0, 6, 1)], AnimationEnd.Holds),
        [P.CorpseDustCloud] = new([new(P.CorpseDustCloud, 0, 5, 1)], AnimationEnd.Holds),
        [P.EyeDoorSweatDrop] = new([new(P.EyeDoorSweatDrop, 0, 5, 1)], AnimationEnd.Holds),
        [P.Smoke] = new([new(P.Smoke, 0, 5, 1)], AnimationEnd.Holds),
        [P.LoopingElevatorPad] = new([new(P.LoopingElevatorPad, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BigExplosion] = new([new(P.BigExplosion, 0, 7, 1)], AnimationEnd.Holds),
        [P.SmallEnergyDrop] = new([new(P.SmallEnergyDrop, 0, 5, 1)], AnimationEnd.Holds),
        [P.BigEnergyDrop] = new([new(P.BigEnergyDrop, 0, 5, 1)], AnimationEnd.Holds),
        [P.Bomb] = new([new(P.Bomb, 0, 5, 1)], AnimationEnd.Holds),
        [P.WeirdEnergyDrop] = new([new(P.WeirdEnergyDrop, 0, 4, 1)], AnimationEnd.Holds),
        [P.RockParticles] = new([new(P.RockParticles, 0, 17, 1)], AnimationEnd.Holds),
        [P.ShortBigDustCloud] = new([new(P.ShortBigDustCloud, 0, 5, 1)], AnimationEnd.Holds),
        [P.ShortBeamCloud] = new([new(P.ShortBeamCloud, 0, 7, 1)], AnimationEnd.Holds),
        [P.MediumBeamCloud] = new([new(P.MediumBeamCloud, 0, 8, 1)], AnimationEnd.Holds),
        [P.BigDustCloud] = new([new(P.BigDustCloud, 0, 9, 1)], AnimationEnd.Holds),
        [P.LongBeam] = new([new(P.LongBeam, 0, 32, 1)], AnimationEnd.Holds),
        [P.FlickeringLongBeam] = new([new(P.FlickeringLongBeam, 0, 4, 1)], AnimationEnd.Holds),
        [P.DraygonBreathBubbles] = new([new(P.DraygonBreathBubbles, 0, 10, 1)], AnimationEnd.Holds),
        [P.SaveStationElectricity] = new([new(P.SaveStationElectricity, 0, 9, 1)], AnimationEnd.Holds),
        [P.ExpandingGate] = new([new(P.ExpandingGate, 0, 9, 1)], AnimationEnd.Holds),
        [P.ContractingGate] = new([new(P.ContractingGate, 0, 9, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MiscDustProjectileSegment> StartingWith(MiscDustProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
