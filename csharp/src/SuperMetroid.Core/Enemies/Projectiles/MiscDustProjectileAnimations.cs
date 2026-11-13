using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Samus.Projectiles.MiscDustProjectileProgram;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The thirty misc-dust explosion, smoke, dust, beam and drop programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class MiscDustProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MiscDustProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<MiscDustProjectileProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.BeamCharge] =
        [
            Show(EnemyProjectileFrameId.MiscDustBeamChargeFrame00, 3),
            Show(EnemyProjectileFrameId.MiscDustBeamChargeFrame01, 3),
            Show(EnemyProjectileFrameId.MiscDustBeamChargeFrame02, 3),
            Show(EnemyProjectileFrameId.MiscDustBeamChargeFrame03, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ElbowChargeParticle] =
        [
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFrame00, 5),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFrame01, 4),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFrame02, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFrame03, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFrame04, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFrame05, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ElbowChargeEnergy] =
        [
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFrame00, 4),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFrame01, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFrame02, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFrame03, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFrame04, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFrame05, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFrame06, 12),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFrame00, 4),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFrame01, 6),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFrame02, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFrame03, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFrame04, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFrame05, 6),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.RainbowBeamExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFrame00, 3),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFrame01, 3),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFrame02, 4),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFrame03, 4),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFrame04, 4),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.BeamTrail] =
        [
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFrame00, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFrame01, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFrame02, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFrame03, 8),
            Show(EnemyProjectileFrameId.MiscDustBeamTrailFrame04, 24),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.DudShot] =
        [
            Show(EnemyProjectileFrameId.MiscDustDudShotFrame00, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFrame01, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFrame02, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFrame03, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFrame04, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFrame05, 4),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.PowerBomb] =
        [
            Show(EnemyProjectileFrameId.MiscDustPowerBombFrame00, 5),
            Show(EnemyProjectileFrameId.MiscDustPowerBombFrame01, 5),
            Show(EnemyProjectileFrameId.MiscDustPowerBombFrame02, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ElevatorPad] =
        [
            Show(EnemyProjectileFrameId.MiscDustElevatorPadFrame00, 1),
            Show(EnemyProjectileFrameId.MiscDustElevatorPadFrame01, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SmallDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFrame00, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFrame01, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFrame02, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFrame03, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFrame04, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.CorpseDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFrame00, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFrame01, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFrame02, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFrame03, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.EyeDoorSweatDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFrame00, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFrame01, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFrame02, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFrame03, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.Smoke] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmokeFrame00, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeFrame01, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeFrame02, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeFrame03, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.LoopingElevatorPad] =
        [
            Show(EnemyProjectileFrameId.MiscDustLoopingElevatorPadFrame00, 1),
            Show(EnemyProjectileFrameId.MiscDustLoopingElevatorPadFrame01, 1),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFrame00, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFrame01, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFrame02, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFrame03, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFrame04, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFrame05, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SmallEnergyDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropFrame00, 8),
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropFrame01, 8),
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropFrame02, 8),
            Show(EnemyProjectileFrameId.MiscDustSmallEnergyDropFrame03, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.BigEnergyDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropFrame00, 8),
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropFrame01, 8),
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropFrame02, 8),
            Show(EnemyProjectileFrameId.MiscDustBigEnergyDropFrame03, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.Bomb] =
        [
            Show(EnemyProjectileFrameId.MiscDustBombFrame00, 5),
            Show(EnemyProjectileFrameId.MiscDustBombFrame01, 5),
            Show(EnemyProjectileFrameId.MiscDustBombFrame02, 5),
            Show(EnemyProjectileFrameId.MiscDustBombFrame03, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.WeirdEnergyDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustWeirdEnergyDropFrame00, 16),
            Show(EnemyProjectileFrameId.MiscDustWeirdEnergyDropFrame01, 16),
            Show(EnemyProjectileFrameId.MiscDustWeirdEnergyDropFrame02, 16),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.RockParticles] =
        [
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame00, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame01, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame02, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame03, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame04, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame05, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame06, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame07, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame08, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame09, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame10, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame11, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame12, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame13, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame14, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFrame15, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFrame00, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFrame01, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFrame02, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFrame03, 2),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ShortBeamCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFrame00, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFrame01, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFrame02, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFrame03, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFrame04, 3),
            Show(EnemyProjectileFrameId.MiscDustShortBeamCloudFrame05, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.MediumBeamCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFrame00, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFrame01, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFrame02, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFrame03, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFrame04, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFrame05, 3),
            Show(EnemyProjectileFrameId.MiscDustMediumBeamCloudFrame06, 3),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame00, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame01, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame02, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame03, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame04, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame05, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame06, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFrame07, 5),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.LongBeam] =
        [
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame00, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame01, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame02, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame03, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame04, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame05, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame06, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame07, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame08, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame09, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame10, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame11, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame12, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame13, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame14, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame15, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame16, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame17, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame18, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame19, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame20, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame21, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame22, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame23, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame24, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame25, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame26, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame27, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame28, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame29, 1),
            Show(EnemyProjectileFrameId.MiscDustLongBeamFrame30, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.FlickeringLongBeam] =
        [
            Show(EnemyProjectileFrameId.MiscDustFlickeringLongBeamFrame00, 1),
            Show(EnemyProjectileFrameId.MiscDustFlickeringLongBeamFrame01, 1),
            Show(EnemyProjectileFrameId.MiscDustFlickeringLongBeamFrame02, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.DraygonBreathBubbles] =
        [
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame00, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame01, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame02, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame03, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame04, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame05, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame06, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame07, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFrame08, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.SaveStationElectricity] =
        [
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame00, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame01, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame02, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame03, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame04, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame05, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame06, 1),
            Show(EnemyProjectileFrameId.MiscDustSaveStationElectricityFrame07, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ExpandingGate] =
        [
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame00, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame01, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame02, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame03, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame04, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame05, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame06, 16),
            Show(EnemyProjectileFrameId.MiscDustExpandingGateFrame07, 16),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.ContractingGate] =
        [
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame00, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame01, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame02, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame03, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame04, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame05, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame06, 4),
            Show(EnemyProjectileFrameId.MiscDustContractingGateFrame07, 4),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MiscDustProjectileProgram, Animation<MiscDustProjectileProgram>> StartingAt { get; } = new Dictionary<MiscDustProjectileProgram, Animation<MiscDustProjectileProgram>>
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
    internal static Animation<MiscDustProjectileProgram> StartingWith(MiscDustProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
