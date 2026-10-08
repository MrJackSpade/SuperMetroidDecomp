namespace SuperMetroid.Core.Game;

/// <summary>
/// One-based indices into the cartridge's bank-$85 gameplay-message definition table.
/// Keeping the native identities here makes callers describe the message they request
/// instead of scattering otherwise opaque byte literals through PLM and frontend code.
/// </summary>
public static class GameplayMessageIds
{
    /// <summary>Requests the Energy Tank acquisition panel after increasing energy capacity.</summary>
    public const GameplayMessageId EnergyTank = GameplayMessageId.EnergyTank;
    /// <summary>Requests the Missile Tank acquisition panel after increasing ordinary missile capacity.</summary>
    public const GameplayMessageId MissileTank = GameplayMessageId.MissileTank;
    /// <summary>Requests the Super Missile Tank acquisition panel after increasing super missile capacity.</summary>
    public const GameplayMessageId SuperMissileTank = GameplayMessageId.SuperMissileTank;
    /// <summary>Requests the Power Bomb Tank acquisition panel after increasing power bomb capacity.</summary>
    public const GameplayMessageId PowerBombTank = GameplayMessageId.PowerBombTank;
    /// <summary>Requests the Grapple Beam acquisition panel and its control hint.</summary>
    public const GameplayMessageId GrappleBeam = GameplayMessageId.GrappleBeam;
    /// <summary>Requests the X-ray Scope acquisition panel and its control hint.</summary>
    public const GameplayMessageId XrayScope = GameplayMessageId.XrayScope;
    /// <summary>Requests the Varia Suit acquisition panel.</summary>
    public const GameplayMessageId VariaSuit = GameplayMessageId.VariaSuit;
    /// <summary>Requests the Spring Ball acquisition panel.</summary>
    public const GameplayMessageId SpringBall = GameplayMessageId.SpringBall;
    /// <summary>Requests the Morphing Ball acquisition panel.</summary>
    public const GameplayMessageId MorphBall = GameplayMessageId.MorphBall;
    /// <summary>Requests the Screw Attack acquisition panel.</summary>
    public const GameplayMessageId ScrewAttack = GameplayMessageId.ScrewAttack;
    /// <summary>Requests the Hi-Jump Boots acquisition panel.</summary>
    public const GameplayMessageId HiJumpBoots = GameplayMessageId.HiJumpBoots;
    /// <summary>Requests the Space Jump acquisition panel.</summary>
    public const GameplayMessageId SpaceJump = GameplayMessageId.SpaceJump;
    /// <summary>Requests the Speed Booster acquisition panel.</summary>
    public const GameplayMessageId SpeedBooster = GameplayMessageId.SpeedBooster;
    /// <summary>Requests the Charge Beam acquisition panel.</summary>
    public const GameplayMessageId ChargeBeam = GameplayMessageId.ChargeBeam;
    /// <summary>Requests the Ice Beam acquisition panel.</summary>
    public const GameplayMessageId IceBeam = GameplayMessageId.IceBeam;
    /// <summary>Requests the Wave Beam acquisition panel.</summary>
    public const GameplayMessageId WaveBeam = GameplayMessageId.WaveBeam;
    /// <summary>Requests the Spazer Beam acquisition panel.</summary>
    public const GameplayMessageId SpazerBeam = GameplayMessageId.SpazerBeam;
    /// <summary>Requests the Plasma Beam acquisition panel.</summary>
    public const GameplayMessageId PlasmaBeam = GameplayMessageId.PlasmaBeam;
    /// <summary>Requests the Bomb acquisition panel after enabling morph-ball bombs.</summary>
    public const GameplayMessageId Bombs = GameplayMessageId.Bombs;
    /// <summary>Requests the map-station completion notice after area data is acquired.</summary>
    public const GameplayMessageId MapDataAccessCompleted = GameplayMessageId.MapDataAccessCompleted;
    /// <summary>Requests the energy-station completion notice after refilling Samus's energy.</summary>
    public const GameplayMessageId EnergyRechargeCompleted = GameplayMessageId.EnergyRechargeCompleted;
    /// <summary>Requests the missile-station completion notice after refilling ammunition.</summary>
    public const GameplayMessageId MissileRechargeCompleted = GameplayMessageId.MissileRechargeCompleted;
    /// <summary>Requests the in-room save-station yes/no prompt.</summary>
    public const GameplayMessageId SaveConfirmation = GameplayMessageId.SaveConfirmation;
    /// <summary>Requests the completion notice emitted after a save-station write succeeds.</summary>
    public const GameplayMessageId SaveCompleted = GameplayMessageId.SaveCompleted;
    /// <summary>Requests the Reserve Tank acquisition panel after increasing reserve capacity.</summary>
    public const GameplayMessageId ReserveTank = GameplayMessageId.ReserveTank;
    /// <summary>Requests the Gravity Suit acquisition panel.</summary>
    public const GameplayMessageId GravitySuit = GameplayMessageId.GravitySuit;
    /// <summary>$85:873D definition; $A2:AB1F requests the gunship save/completion coroutine.</summary>
    public const GameplayMessageId GunshipSaveConfirmation = GameplayMessageId.GunshipSaveConfirmation;
}

/// <summary>
/// Native one-based indices into <c>$85:869B</c>. Ordinary definitions run through Gravity
/// Suit; the gunship uses its additional confirmation definition after the terminator.
/// </summary>
public enum GameplayMessageId : byte
{
    /// <summary>No gameplay message; zero is outside the one-based definition table.</summary>
    None = 0x00,
    /// <summary>Definition 1, the Energy Tank acquisition text and title graphics.</summary>
    EnergyTank = 0x01,
    /// <summary>Definition 2, the Missile Tank acquisition text and title graphics.</summary>
    MissileTank = 0x02,
    /// <summary>Definition 3, the Super Missile Tank acquisition text and title graphics.</summary>
    SuperMissileTank = 0x03,
    /// <summary>Definition 4, the Power Bomb Tank acquisition text and title graphics.</summary>
    PowerBombTank = 0x04,
    /// <summary>Definition 5, the Grapple Beam acquisition text and controller glyph.</summary>
    GrappleBeam = 0x05,
    /// <summary>Definition 6, the X-ray Scope acquisition text and controller glyph.</summary>
    XrayScope = 0x06,
    /// <summary>Definition 7, the Varia Suit acquisition text and title graphics.</summary>
    VariaSuit = 0x07,
    /// <summary>Definition 8, the Spring Ball acquisition text and title graphics.</summary>
    SpringBall = 0x08,
    /// <summary>Definition 9, the Morphing Ball acquisition text and title graphics.</summary>
    MorphBall = 0x09,
    /// <summary>Definition 10, the Screw Attack acquisition text and title graphics.</summary>
    ScrewAttack = 0x0a,
    /// <summary>Definition 11, the Hi-Jump Boots acquisition text and title graphics.</summary>
    HiJumpBoots = 0x0b,
    /// <summary>Definition 12, the Space Jump acquisition text and title graphics.</summary>
    SpaceJump = 0x0c,
    /// <summary>Definition 13, the Speed Booster acquisition text and title graphics.</summary>
    SpeedBooster = 0x0d,
    /// <summary>Definition 14, the Charge Beam acquisition text and title graphics.</summary>
    ChargeBeam = 0x0e,
    /// <summary>Definition 15, the Ice Beam acquisition text and title graphics.</summary>
    IceBeam = 0x0f,
    /// <summary>Definition 16, the Wave Beam acquisition text and title graphics.</summary>
    WaveBeam = 0x10,
    /// <summary>Definition 17, the Spazer Beam acquisition text and title graphics.</summary>
    SpazerBeam = 0x11,
    /// <summary>Definition 18, the Plasma Beam acquisition text and title graphics.</summary>
    PlasmaBeam = 0x12,
    /// <summary>Definition 19, the Bomb acquisition text and title graphics.</summary>
    Bombs = 0x13,
    /// <summary>Definition 20, the map-data-access completion notice.</summary>
    MapDataAccessCompleted = 0x14,
    /// <summary>Definition 21, the energy-recharge completion notice.</summary>
    EnergyRechargeCompleted = 0x15,
    /// <summary>Definition 22, the missile-recharge completion notice.</summary>
    MissileRechargeCompleted = 0x16,
    /// <summary>Definition 23, the save-station confirmation question with yes/no selection.</summary>
    SaveConfirmation = 0x17,
    /// <summary>Definition 24, the notice shown after the save write completes.</summary>
    SaveCompleted = 0x18,
    /// <summary>Definition 25, the Reserve Tank acquisition text and title graphics.</summary>
    ReserveTank = 0x19,
    /// <summary>Definition 26, the Gravity Suit acquisition text and title graphics.</summary>
    GravitySuit = 0x1a,
    /// <summary>$85:873D: ship confirmation followed by saving sound and completion notice on YES.</summary>
    GunshipSaveConfirmation = 0x1c,
}
