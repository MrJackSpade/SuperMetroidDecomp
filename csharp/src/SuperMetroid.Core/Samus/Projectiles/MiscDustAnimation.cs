namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The thirty bank-$86 misc-dust and explosion animations, in the native index order of
/// <c>MiscDust_InstListPointers</c> ($86:E42C). The spawn parameter of
/// <c>EprojInit_DustCloudOrExplosion</c> is this index.
/// </summary>
public enum MiscDustAnimation
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
