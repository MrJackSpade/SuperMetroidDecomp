namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The gameplay messages a message box shows: item acquisitions, station notices and the save prompts. Its values are
/// arbitrary; the import pipeline knows each message's cartridge number.
/// </summary>
public enum GameplayMessageId
{
    /// <summary>No gameplay message.</summary>
    None,
    /// <summary>The Energy Tank acquisition text and title graphics.</summary>
    EnergyTank,
    /// <summary>The Missile Tank acquisition text and title graphics.</summary>
    MissileTank,
    /// <summary>The Super Missile Tank acquisition text and title graphics.</summary>
    SuperMissileTank,
    /// <summary>The Power Bomb Tank acquisition text and title graphics.</summary>
    PowerBombTank,
    /// <summary>The Grapple Beam acquisition text and controller glyph.</summary>
    GrappleBeam,
    /// <summary>The X-ray Scope acquisition text and controller glyph.</summary>
    XrayScope,
    /// <summary>The Varia Suit acquisition text and title graphics.</summary>
    VariaSuit,
    /// <summary>The Spring Ball acquisition text and title graphics.</summary>
    SpringBall,
    /// <summary>The Morphing Ball acquisition text and title graphics.</summary>
    MorphBall,
    /// <summary>The Screw Attack acquisition text and title graphics.</summary>
    ScrewAttack,
    /// <summary>The Hi-Jump Boots acquisition text and title graphics.</summary>
    HiJumpBoots,
    /// <summary>The Space Jump acquisition text and title graphics.</summary>
    SpaceJump,
    /// <summary>The Speed Booster acquisition text and title graphics.</summary>
    SpeedBooster,
    /// <summary>The Charge Beam acquisition text and title graphics.</summary>
    ChargeBeam,
    /// <summary>The Ice Beam acquisition text and title graphics.</summary>
    IceBeam,
    /// <summary>The Wave Beam acquisition text and title graphics.</summary>
    WaveBeam,
    /// <summary>The Spazer Beam acquisition text and title graphics.</summary>
    SpazerBeam,
    /// <summary>The Plasma Beam acquisition text and title graphics.</summary>
    PlasmaBeam,
    /// <summary>The Bomb acquisition text and title graphics.</summary>
    Bombs,
    /// <summary>The map-data-access completion notice.</summary>
    MapDataAccessCompleted,
    /// <summary>The energy-recharge completion notice.</summary>
    EnergyRechargeCompleted,
    /// <summary>The missile-recharge completion notice.</summary>
    MissileRechargeCompleted,
    /// <summary>The save-station confirmation question with yes/no selection.</summary>
    SaveConfirmation,
    /// <summary>The notice shown after the save write completes.</summary>
    SaveCompleted,
    /// <summary>The Reserve Tank acquisition text and title graphics.</summary>
    ReserveTank,
    /// <summary>The Gravity Suit acquisition text and title graphics.</summary>
    GravitySuit,
    /// <summary>The gunship's save confirmation, followed by the saving sound and the completion notice on YES.</summary>
    GunshipSaveConfirmation,
}
