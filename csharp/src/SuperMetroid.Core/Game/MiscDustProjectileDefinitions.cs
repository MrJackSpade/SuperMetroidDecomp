namespace SuperMetroid.Core.Game;

/// <summary>One randomized room-coordinate placement record for bank-$86 misc dust.</summary>
/// <param name="RandomMask">Per-axis masks applied to the random word's low (X) and high (Y) bytes; each is the scatter span minus one.</param>
/// <param name="Base">Signed offset added before the masked random scatter.</param>
internal readonly record struct MiscDustPlacementDefinition(
    Extent16<RoomPixels> RandomMask,
    Offset16<RoomPixels> Base);

/// <summary>
/// The thirty bank-$86 misc-dust and explosion animations, in the native index order of
/// <c>MiscDust_InstListPointers</c> ($86:E42C). The spawn parameter of
/// <c>EprojInit_DustCloudOrExplosion</c> is this index.
/// </summary>
public enum MiscDustAnimation : ushort
{
    /// <summary>Unused beam-charge sparkle ($86:E0EE).</summary>
    UnusedBeamCharge,
    /// <summary>Mother Brain's elbow charge particle.</summary>
    MotherBrainElbowChargeParticle,
    /// <summary>Mother Brain's elbow charge energy.</summary>
    MotherBrainElbowChargeEnergy,
    /// <summary>Small explosion.</summary>
    SmallExplosion,
    /// <summary>Mother Brain's rainbow-beam explosion.</summary>
    MotherBrainRainbowBeamExplosion,
    /// <summary>Unused beam trail ($86:E168).</summary>
    UnusedBeamTrail,
    /// <summary>Dud shot: tiny explosion.</summary>
    DudShotTinyExplosion,
    /// <summary>Unused power bomb ($86:E198).</summary>
    UnusedPowerBomb,
    /// <summary>Unused elevator pad ($86:E1A6).</summary>
    UnusedElevatorPad,
    /// <summary>Small dust cloud.</summary>
    SmallDustCloud,
    /// <summary>Corpse dust cloud.</summary>
    CorpseDustCloud,
    /// <summary>Eye-door sweat drop.</summary>
    EyeDoorSweatDrop,
    /// <summary>Smoke.</summary>
    Smoke,
    /// <summary>Unused small energy drop ($86:E222).</summary>
    UnusedSmallEnergyDrop,
    /// <summary>Unused big energy drop ($86:E22E).</summary>
    UnusedBigEnergyDrop,
    /// <summary>Unused bomb ($86:E246).</summary>
    UnusedBomb,
    /// <summary>Unused weird energy drop ($86:E258).</summary>
    UnusedWeirdEnergyDrop,
    /// <summary>Rock particles.</summary>
    RockParticles,
    /// <summary>Short big dust cloud.</summary>
    ShortBigDustCloud,
    /// <summary>Unused cloud with a short beam ($86:E2BA).</summary>
    UnusedCloudShortBeam,
    /// <summary>Unused cloud with a medium beam ($86:E2D4).</summary>
    UnusedCloudMediumBeam,
    /// <summary>Big dust cloud.</summary>
    BigDustCloud,
    /// <summary>Unused long beam ($86:E314).</summary>
    UnusedLongBeam,
    /// <summary>Unused flickering long beam ($86:E392).</summary>
    UnusedFlickerLongBeam,
    /// <summary>Long Draygon breath bubbles.</summary>
    LongDraygonBreathBubbles,
    /// <summary>Unused save-station electricity ($86:E3C6).</summary>
    UnusedSaveStationElectricity,
    /// <summary>Unused expanding vertical gate ($86:E3E8).</summary>
    UnusedExpandVerticalGate,
    /// <summary>Unused contracting vertical gate ($86:E40A).</summary>
    UnusedContractVerticalGate,
    /// <summary>Unused looping elevator pad ($86:E1FC).</summary>
    UnusedLoopingElevatorPad,
    /// <summary>Big explosion.</summary>
    BigExplosion,
}

/// <summary>Compiled cartridge definitions shared by room-graphics dust and explosions.</summary>
internal static class MiscDustProjectileDefinitions
{
    /// <summary>Returns one of the thirty authored misc-dust animation lists.</summary>
    internal static ushort InstructionList(MiscDustAnimation animation) =>
        EnemyProjectileInstructionMechanicsDefinitions.MiscDustInitialPointer(animation);

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
