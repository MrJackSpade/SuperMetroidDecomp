namespace SuperMetroid.Core.Game;

/// <summary>
/// One-based indices into the cartridge's bank-$85 gameplay-message definition table.
/// Keeping the native identities here makes callers describe the message they request
/// instead of scattering otherwise opaque byte literals through PLM and frontend code.
/// </summary>
public static class GameplayMessageIds
{
    public const GameplayMessageId EnergyTank = GameplayMessageId.EnergyTank;
    public const GameplayMessageId MissileTank = GameplayMessageId.MissileTank;
    public const GameplayMessageId SuperMissileTank = GameplayMessageId.SuperMissileTank;
    public const GameplayMessageId PowerBombTank = GameplayMessageId.PowerBombTank;
    public const GameplayMessageId GrappleBeam = GameplayMessageId.GrappleBeam;
    public const GameplayMessageId XrayScope = GameplayMessageId.XrayScope;
    public const GameplayMessageId VariaSuit = GameplayMessageId.VariaSuit;
    public const GameplayMessageId SpringBall = GameplayMessageId.SpringBall;
    public const GameplayMessageId MorphBall = GameplayMessageId.MorphBall;
    public const GameplayMessageId ScrewAttack = GameplayMessageId.ScrewAttack;
    public const GameplayMessageId HiJumpBoots = GameplayMessageId.HiJumpBoots;
    public const GameplayMessageId SpaceJump = GameplayMessageId.SpaceJump;
    public const GameplayMessageId SpeedBooster = GameplayMessageId.SpeedBooster;
    public const GameplayMessageId ChargeBeam = GameplayMessageId.ChargeBeam;
    public const GameplayMessageId IceBeam = GameplayMessageId.IceBeam;
    public const GameplayMessageId WaveBeam = GameplayMessageId.WaveBeam;
    public const GameplayMessageId SpazerBeam = GameplayMessageId.SpazerBeam;
    public const GameplayMessageId PlasmaBeam = GameplayMessageId.PlasmaBeam;
    public const GameplayMessageId Bombs = GameplayMessageId.Bombs;
    public const GameplayMessageId MapDataAccessCompleted = GameplayMessageId.MapDataAccessCompleted;
    public const GameplayMessageId EnergyRechargeCompleted = GameplayMessageId.EnergyRechargeCompleted;
    public const GameplayMessageId MissileRechargeCompleted = GameplayMessageId.MissileRechargeCompleted;
    public const GameplayMessageId SaveConfirmation = GameplayMessageId.SaveConfirmation;
    public const GameplayMessageId SaveCompleted = GameplayMessageId.SaveCompleted;
    public const GameplayMessageId ReserveTank = GameplayMessageId.ReserveTank;
    public const GameplayMessageId GravitySuit = GameplayMessageId.GravitySuit;

    /// <summary>
    /// Converts a byte read from a translated cartridge owner into the closed retail
    /// message catalog. The diagnostic identifies both the raw ID and the owner that read
    /// it, rather than failing later at an unrelated definition-table access.
    /// </summary>
    public static GameplayMessageId FromCartridge(byte value, string sourceContext)
    {
        if (value is < (byte)GameplayMessageId.EnergyTank or > (byte)GameplayMessageId.GravitySuit)
        {
            throw new NotSupportedException(
                $"Gameplay message ${value:X2} from {sourceContext} is not translated.");
        }

        return (GameplayMessageId)value;
    }
}

/// <summary>
/// Native one-based indices into <c>$85:869B</c>. Every value through Gravity Suit has a
/// complete definition consumed by the shared translated message coroutine.
/// </summary>
public enum GameplayMessageId : byte
{
    None = 0x00,
    EnergyTank = 0x01,
    MissileTank = 0x02,
    SuperMissileTank = 0x03,
    PowerBombTank = 0x04,
    GrappleBeam = 0x05,
    XrayScope = 0x06,
    VariaSuit = 0x07,
    SpringBall = 0x08,
    MorphBall = 0x09,
    ScrewAttack = 0x0a,
    HiJumpBoots = 0x0b,
    SpaceJump = 0x0c,
    SpeedBooster = 0x0d,
    ChargeBeam = 0x0e,
    IceBeam = 0x0f,
    WaveBeam = 0x10,
    SpazerBeam = 0x11,
    PlasmaBeam = 0x12,
    Bombs = 0x13,
    MapDataAccessCompleted = 0x14,
    EnergyRechargeCompleted = 0x15,
    MissileRechargeCompleted = 0x16,
    SaveConfirmation = 0x17,
    SaveCompleted = 0x18,
    ReserveTank = 0x19,
    GravitySuit = 0x1a,
}
