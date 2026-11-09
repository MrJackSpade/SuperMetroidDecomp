namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program and smoke placement of each miscellaneous dust projectile.</summary>
internal static class MiscDustProjectiles
{
    /// <summary>The program each of the thirty misc-dust selectors starts.</summary>
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

    /// <summary>Point, centered 8/16-pixel squares, then centered 16x32/32x64 smoke boxes.</summary>
    internal static MiscDustPlacementDefinition SmokePlacement(int placementIndex)
    {
        if (placementIndex >= 5)
            throw new InvalidDataException($"Misc-dust placement index {placementIndex} exceeds the five placement records.");
        // The point placement reads no bits: its fields have one outcome, zero.
        if (placementIndex == 0) return new(new(0, 0), new(YScatterShift, 0), default);
        bool tall = placementIndex >= 3;
        int xWidth = placementIndex + (tall ? 1 : 2);
        int yWidth = tall ? xWidth + 1 : xWidth;
        // Each box is centred: its base is minus half the span its field scatters across.
        return new(
            new RandomSampleField(0, xWidth),
            new RandomSampleField(YScatterShift, yWidth),
            new Offset16<RoomPixels>(CoordinateAxis.AsSigned(-(1 << (xWidth - 1))), CoordinateAxis.AsSigned(-(1 << (yWidth - 1)))));
    }

    /// <summary>Where the sample's bits that scatter the smoke's Y axis start.</summary>
    private const int YScatterShift = 8;
}
