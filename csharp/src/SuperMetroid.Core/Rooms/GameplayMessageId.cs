namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Native one-based indices into <c>$85:869B</c>. Ordinary definitions run through Gravity
/// Suit; the gunship uses its additional confirmation definition after the terminator.
/// </summary>
public enum GameplayMessageId
{
    /// <summary>No gameplay message; zero is outside the one-based definition table.</summary>
    None = 0,
    /// <summary>Definition 1, the Energy Tank acquisition text and title graphics.</summary>
    EnergyTank = 1,
    /// <summary>Definition 2, the Missile Tank acquisition text and title graphics.</summary>
    MissileTank = 2,
    /// <summary>Definition 3, the Super Missile Tank acquisition text and title graphics.</summary>
    SuperMissileTank = 3,
    /// <summary>Definition 4, the Power Bomb Tank acquisition text and title graphics.</summary>
    PowerBombTank = 4,
    /// <summary>Definition 5, the Grapple Beam acquisition text and controller glyph.</summary>
    GrappleBeam = 5,
    /// <summary>Definition 6, the X-ray Scope acquisition text and controller glyph.</summary>
    XrayScope = 6,
    /// <summary>Definition 7, the Varia Suit acquisition text and title graphics.</summary>
    VariaSuit = 7,
    /// <summary>Definition 8, the Spring Ball acquisition text and title graphics.</summary>
    SpringBall = 8,
    /// <summary>Definition 9, the Morphing Ball acquisition text and title graphics.</summary>
    MorphBall = 9,
    /// <summary>Definition 10, the Screw Attack acquisition text and title graphics.</summary>
    ScrewAttack = 10,
    /// <summary>Definition 11, the Hi-Jump Boots acquisition text and title graphics.</summary>
    HiJumpBoots = 11,
    /// <summary>Definition 12, the Space Jump acquisition text and title graphics.</summary>
    SpaceJump = 12,
    /// <summary>Definition 13, the Speed Booster acquisition text and title graphics.</summary>
    SpeedBooster = 13,
    /// <summary>Definition 14, the Charge Beam acquisition text and title graphics.</summary>
    ChargeBeam = 14,
    /// <summary>Definition 15, the Ice Beam acquisition text and title graphics.</summary>
    IceBeam = 15,
    /// <summary>Definition 16, the Wave Beam acquisition text and title graphics.</summary>
    WaveBeam = 16,
    /// <summary>Definition 17, the Spazer Beam acquisition text and title graphics.</summary>
    SpazerBeam = 17,
    /// <summary>Definition 18, the Plasma Beam acquisition text and title graphics.</summary>
    PlasmaBeam = 18,
    /// <summary>Definition 19, the Bomb acquisition text and title graphics.</summary>
    Bombs = 19,
    /// <summary>Definition 20, the map-data-access completion notice.</summary>
    MapDataAccessCompleted = 20,
    /// <summary>Definition 21, the energy-recharge completion notice.</summary>
    EnergyRechargeCompleted = 21,
    /// <summary>Definition 22, the missile-recharge completion notice.</summary>
    MissileRechargeCompleted = 22,
    /// <summary>Definition 23, the save-station confirmation question with yes/no selection.</summary>
    SaveConfirmation = 23,
    /// <summary>Definition 24, the notice shown after the save write completes.</summary>
    SaveCompleted = 24,
    /// <summary>Definition 25, the Reserve Tank acquisition text and title graphics.</summary>
    ReserveTank = 25,
    /// <summary>Definition 26, the Gravity Suit acquisition text and title graphics.</summary>
    GravitySuit = 26,
    /// <summary>$85:873D: ship confirmation followed by saving sound and completion notice on YES.</summary>
    GunshipSaveConfirmation = 28,
}
