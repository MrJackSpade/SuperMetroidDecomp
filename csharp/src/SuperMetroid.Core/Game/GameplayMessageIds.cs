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
