using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Compiled cartridge definitions shared by room-graphics dust and explosions.</summary>
internal static class MiscDustProjectileDefinitions
{
    /// <summary>The program each of the thirty native misc-dust selectors ($86:E42C) starts.</summary>
    internal static MiscDustProjectileProgram Program(MiscDustAnimation animation) => animation switch
    {
        MiscDustAnimation.UnusedBeamCharge => MiscDustProjectileProgram.BeamCharge,
        MiscDustAnimation.MotherBrainElbowChargeParticle => MiscDustProjectileProgram.ElbowChargeParticle,
        MiscDustAnimation.MotherBrainElbowChargeEnergy => MiscDustProjectileProgram.ElbowChargeEnergy,
        MiscDustAnimation.SmallExplosion => MiscDustProjectileProgram.SmallExplosion,
        MiscDustAnimation.MotherBrainRainbowBeamExplosion => MiscDustProjectileProgram.RainbowBeamExplosion,
        MiscDustAnimation.UnusedBeamTrail => MiscDustProjectileProgram.BeamTrail,
        MiscDustAnimation.DudShotTinyExplosion => MiscDustProjectileProgram.DudShot,
        MiscDustAnimation.UnusedPowerBomb => MiscDustProjectileProgram.PowerBomb,
        MiscDustAnimation.UnusedElevatorPad => MiscDustProjectileProgram.ElevatorPad,
        MiscDustAnimation.SmallDustCloud => MiscDustProjectileProgram.SmallDustCloud,
        MiscDustAnimation.CorpseDustCloud => MiscDustProjectileProgram.CorpseDustCloud,
        MiscDustAnimation.EyeDoorSweatDrop => MiscDustProjectileProgram.EyeDoorSweatDrop,
        MiscDustAnimation.Smoke => MiscDustProjectileProgram.Smoke,
        MiscDustAnimation.UnusedSmallEnergyDrop => MiscDustProjectileProgram.SmallEnergyDrop,
        MiscDustAnimation.UnusedBigEnergyDrop => MiscDustProjectileProgram.BigEnergyDrop,
        MiscDustAnimation.UnusedBomb => MiscDustProjectileProgram.Bomb,
        MiscDustAnimation.UnusedWeirdEnergyDrop => MiscDustProjectileProgram.WeirdEnergyDrop,
        MiscDustAnimation.RockParticles => MiscDustProjectileProgram.RockParticles,
        MiscDustAnimation.ShortBigDustCloud => MiscDustProjectileProgram.ShortBigDustCloud,
        MiscDustAnimation.UnusedCloudShortBeam => MiscDustProjectileProgram.ShortBeamCloud,
        MiscDustAnimation.UnusedCloudMediumBeam => MiscDustProjectileProgram.MediumBeamCloud,
        MiscDustAnimation.BigDustCloud => MiscDustProjectileProgram.BigDustCloud,
        MiscDustAnimation.UnusedLongBeam => MiscDustProjectileProgram.LongBeam,
        MiscDustAnimation.UnusedFlickerLongBeam => MiscDustProjectileProgram.FlickeringLongBeam,
        MiscDustAnimation.LongDraygonBreathBubbles => MiscDustProjectileProgram.DraygonBreathBubbles,
        MiscDustAnimation.UnusedSaveStationElectricity => MiscDustProjectileProgram.SaveStationElectricity,
        MiscDustAnimation.UnusedExpandVerticalGate => MiscDustProjectileProgram.ExpandingGate,
        MiscDustAnimation.UnusedContractVerticalGate => MiscDustProjectileProgram.ContractingGate,
        MiscDustAnimation.UnusedLoopingElevatorPad => MiscDustProjectileProgram.LoopingElevatorPad,
        MiscDustAnimation.BigExplosion => MiscDustProjectileProgram.BigExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined misc-dust animation."),
    };

    /// <summary>$86:E47E: point, centered 8/16-pixel squares, then centered 16x32/32x64 smoke boxes.</summary>
    internal static MiscDustPlacementDefinition SmokePlacement(ushort placementIndex)
    {
        if (placementIndex >= 5)
            throw new InvalidDataException($"Bank-$86 misc-dust placement index {placementIndex} exceeds five native records.");
        if (placementIndex == 0) return default;
        bool tall = placementIndex >= 3;
        int xRadius = 1 << (placementIndex + (tall ? 0 : 1));
        int yRadius = tall ? xRadius * 2 : xRadius;
        return new(
            new Extent16<RoomPixels>((ushort)(xRadius * 2 - 1), (ushort)(yRadius * 2 - 1)),
            new Offset16<RoomPixels>((short)-xRadius, (short)-yRadius));
    }
}
