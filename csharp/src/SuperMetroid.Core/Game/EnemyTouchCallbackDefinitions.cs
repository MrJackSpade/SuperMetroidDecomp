namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal no-op touch callback identities found in the complete retail enemy-header and
/// extended-hitbox inventory. These are callback identities, not generic opcode probes.
/// </summary>
public static class EnemyTouchCallbackDefinitions
{
    /// <summary>$A2:804C, bank-A2's common RTL callback.</summary>
    public const int BankA2NoOp = 0xa2804c;

    /// <summary>$A3:804C, bank-A3's common RTL callback.</summary>
    public const int BankA3NoOp = 0xa3804c;

    /// <summary><c>RTL_A39F07</c> at $A3:9F07: tripper and suspensor-platform touch callback.</summary>
    public const int TripperPlatformNoOp = 0xa39f07;

    /// <summary><c>RTL_A4B950</c> at $A4:B950: Crocomire's harmless body/header touch callback.</summary>
    public const int CrocomireNoOp = 0xa4b950;

    /// <summary>$A5:804C, bank-local common RTL used by Draygon and Spore Spawn components.</summary>
    public const int BankA5NoOp = 0xa5804c;

    /// <summary>$A6:804C, bank-A6's common RTL callback.</summary>
    public const int BankA6NoOp = 0xa6804c;

    /// <summary><c>RTL_A6F920</c> at $A6:F920: Ceres-door touch callback.</summary>
    public const int CeresDoorNoOp = 0xa6f920;

    /// <summary>$A7:804C, bank-local common RTL used by Phantoon components.</summary>
    public const int BankA7NoOp = 0xa7804c;

    /// <summary><c>RTL_A7948F</c> at $A7:948F: Kraid-component touch callback.</summary>
    public const int KraidComponentNoOp = 0xa7948f;

    /// <summary>$A8:804C, bank-A8's common RTL callback.</summary>
    public const int BankA8NoOp = 0xa8804c;

    /// <summary>$A9:804C, bank-A9's common RTL callback.</summary>
    public const int BankA9NoOp = 0xa9804c;

    /// <summary><c>RTL_A9B5C5</c> at $A9:B5C5: Mother Brain body's harmless touch callback.</summary>
    public const int MotherBrainBodyNoOp = 0xa9b5c5;

    /// <summary>$AA:804C, bank-AA's common RTL callback.</summary>
    public const int BankAaNoOp = 0xaa804c;

    /// <summary><c>RTL_AAE7DB</c> at $AA:E7DB: Chozo-statue touch callback.</summary>
    public const int ChozoStatueNoOp = 0xaae7db;

    /// <summary>$B3:804C, bank-B3's common RTL callback.</summary>
    public const int BankB3NoOp = 0xb3804c;

    /// <summary>Classifies the fifteen literal RTL touch callbacks in the pinned retail inventory.</summary>
    public static bool IsLiteralNoOp(byte bank, ushort pointer) => ((bank << 16) | pointer) is
        BankA2NoOp or
        BankA3NoOp or
        TripperPlatformNoOp or
        CrocomireNoOp or
        BankA5NoOp or
        BankA6NoOp or
        CeresDoorNoOp or
        BankA7NoOp or
        KraidComponentNoOp or
        BankA8NoOp or
        BankA9NoOp or
        MotherBrainBodyNoOp or
        BankAaNoOp or
        ChozoStatueNoOp or
        BankB3NoOp;
}
