namespace SuperMetroid.Core.Game;

/// <summary>
/// One-based indices into the cartridge's bank-$85 gameplay-message definition table.
/// Keeping the native identities here makes callers describe the message they request
/// instead of scattering otherwise opaque byte literals through PLM and frontend code.
/// </summary>
public static class GameplayMessageIds
{
    public const byte EnergyTank = 0x01;
    public const byte MissileTank = 0x02;
    public const byte SuperMissileTank = 0x03;
    public const byte PowerBombTank = 0x04;
    public const byte GrappleBeam = 0x05;
    public const byte XrayScope = 0x06;
    public const byte VariaSuit = 0x07;
    public const byte SpringBall = 0x08;
    public const byte MorphBall = 0x09;
    public const byte ScrewAttack = 0x0a;
    public const byte HiJumpBoots = 0x0b;
    public const byte SpaceJump = 0x0c;
    public const byte SpeedBooster = 0x0d;
    public const byte ChargeBeam = 0x0e;
    public const byte IceBeam = 0x0f;
    public const byte WaveBeam = 0x10;
    public const byte SpazerBeam = 0x11;
    public const byte PlasmaBeam = 0x12;
    public const byte Bombs = 0x13;
    public const byte GravitySuit = 0x14;
    public const byte MapDataAccessCompleted = 0x14;
    public const byte EnergyRechargeCompleted = 0x15;
    public const byte MissileRechargeCompleted = 0x16;
    public const byte SaveConfirmation = 0x17;
    public const byte SaveCompleted = 0x18;
    public const byte ReserveTank = 0x19;
}
