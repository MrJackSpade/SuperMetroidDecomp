namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program and smoke placement of each miscellaneous dust projectile.</summary>
internal static class MiscDustProjectiles
{
    /// <summary>The program each of the thirty misc-dust selectors starts.</summary>
    internal static MiscDustProjectileSegment Animation(MiscDustAnimation animation) => animation switch
    {
        MiscDustAnimation.UnusedBeamCharge => MiscDustProjectileSegment.BeamCharge,
        MiscDustAnimation.MotherBrainElbowChargeParticle => MiscDustProjectileSegment.ElbowChargeParticle,
        MiscDustAnimation.MotherBrainElbowChargeEnergy => MiscDustProjectileSegment.ElbowChargeEnergy,
        MiscDustAnimation.SmallExplosion => MiscDustProjectileSegment.SmallExplosion,
        MiscDustAnimation.MotherBrainRainbowBeamExplosion => MiscDustProjectileSegment.RainbowBeamExplosion,
        MiscDustAnimation.UnusedBeamTrail => MiscDustProjectileSegment.BeamTrail,
        MiscDustAnimation.DudShotTinyExplosion => MiscDustProjectileSegment.DudShot,
        MiscDustAnimation.UnusedPowerBomb => MiscDustProjectileSegment.PowerBomb,
        MiscDustAnimation.UnusedElevatorPad => MiscDustProjectileSegment.ElevatorPad,
        MiscDustAnimation.SmallDustCloud => MiscDustProjectileSegment.SmallDustCloud,
        MiscDustAnimation.CorpseDustCloud => MiscDustProjectileSegment.CorpseDustCloud,
        MiscDustAnimation.EyeDoorSweatDrop => MiscDustProjectileSegment.EyeDoorSweatDrop,
        MiscDustAnimation.Smoke => MiscDustProjectileSegment.Smoke,
        MiscDustAnimation.UnusedSmallEnergyDrop => MiscDustProjectileSegment.SmallEnergyDrop,
        MiscDustAnimation.UnusedBigEnergyDrop => MiscDustProjectileSegment.BigEnergyDrop,
        MiscDustAnimation.UnusedBomb => MiscDustProjectileSegment.Bomb,
        MiscDustAnimation.UnusedWeirdEnergyDrop => MiscDustProjectileSegment.WeirdEnergyDrop,
        MiscDustAnimation.RockParticles => MiscDustProjectileSegment.RockParticles,
        MiscDustAnimation.ShortBigDustCloud => MiscDustProjectileSegment.ShortBigDustCloud,
        MiscDustAnimation.UnusedCloudShortBeam => MiscDustProjectileSegment.ShortBeamCloud,
        MiscDustAnimation.UnusedCloudMediumBeam => MiscDustProjectileSegment.MediumBeamCloud,
        MiscDustAnimation.BigDustCloud => MiscDustProjectileSegment.BigDustCloud,
        MiscDustAnimation.UnusedLongBeam => MiscDustProjectileSegment.LongBeam,
        MiscDustAnimation.UnusedFlickerLongBeam => MiscDustProjectileSegment.FlickeringLongBeam,
        MiscDustAnimation.LongDraygonBreathBubbles => MiscDustProjectileSegment.DraygonBreathBubbles,
        MiscDustAnimation.UnusedSaveStationElectricity => MiscDustProjectileSegment.SaveStationElectricity,
        MiscDustAnimation.UnusedExpandVerticalGate => MiscDustProjectileSegment.ExpandingGate,
        MiscDustAnimation.UnusedContractVerticalGate => MiscDustProjectileSegment.ContractingGate,
        MiscDustAnimation.UnusedLoopingElevatorPad => MiscDustProjectileSegment.LoopingElevatorPad,
        MiscDustAnimation.BigExplosion => MiscDustProjectileSegment.BigExplosion,
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
