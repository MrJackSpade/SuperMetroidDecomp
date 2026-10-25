using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies.Callbacks;

/// <summary>
/// Literal no-op touch callback identities found in the complete retail enemy-header and
/// extended-hitbox inventory. These are callback identities, not generic opcode probes.
/// </summary>
internal static class EnemyTouchCallbackDefinitions
{
    /// <summary>$A2:804C, bank-A2's common RTL callback.</summary>
    public static readonly SnesAddress BankA2NoOp = new(0xa2, 0x804c);

    /// <summary>$A3:804C, bank-A3's common RTL callback.</summary>
    public static readonly SnesAddress BankA3NoOp = new(0xa3, 0x804c);

    /// <summary><c>RTL_A39F07</c> at $A3:9F07: tripper and suspensor-platform touch callback.</summary>
    public static readonly SnesAddress TripperPlatformNoOp = new(0xa3, 0x9f07);

    /// <summary><c>RTL_A4B950</c> at $A4:B950: Crocomire's harmless body/header touch callback.</summary>
    public static readonly SnesAddress CrocomireNoOp = new(0xa4, 0xb950);

    /// <summary>$A5:804C, bank-local common RTL used by Draygon and Spore Spawn components.</summary>
    public static readonly SnesAddress BankA5NoOp = new(0xa5, 0x804c);

    /// <summary>$A6:804C, bank-A6's common RTL callback.</summary>
    public static readonly SnesAddress BankA6NoOp = new(0xa6, 0x804c);

    /// <summary><c>RTL_A6F920</c> at $A6:F920: Ceres-door touch callback.</summary>
    public static readonly SnesAddress CeresDoorNoOp = new(0xa6, 0xf920);

    /// <summary>$A7:804C, bank-local common RTL used by Phantoon components.</summary>
    public static readonly SnesAddress BankA7NoOp = new(0xa7, 0x804c);

    /// <summary><c>RTL_A7948F</c> at $A7:948F: Kraid-component touch callback.</summary>
    public static readonly SnesAddress KraidComponentNoOp = new(0xa7, 0x948f);

    /// <summary>$A8:804C, bank-A8's common RTL callback.</summary>
    public static readonly SnesAddress BankA8NoOp = new(0xa8, 0x804c);

    /// <summary>$A9:804C, bank-A9's common RTL callback.</summary>
    public static readonly SnesAddress BankA9NoOp = new(0xa9, 0x804c);

    /// <summary><c>RTL_A9B5C5</c> at $A9:B5C5: Mother Brain body's harmless touch callback.</summary>
    public static readonly SnesAddress MotherBrainBodyNoOp = new(0xa9, 0xb5c5);

    /// <summary>$AA:804C, bank-AA's common RTL callback.</summary>
    public static readonly SnesAddress BankAaNoOp = new(0xaa, 0x804c);

    /// <summary><c>RTL_AAE7DB</c> at $AA:E7DB: Chozo-statue touch callback.</summary>
    public static readonly SnesAddress ChozoStatueNoOp = new(0xaa, 0xe7db);

    /// <summary>$B3:804C, bank-B3's common RTL callback.</summary>
    public static readonly SnesAddress BankB3NoOp = new(0xb3, 0x804c);

    /// <summary>The fifteen literal-RTL touch callbacks in the pinned retail inventory.</summary>
    public static readonly IReadOnlySet<SnesAddress> LiteralNoOps = new HashSet<SnesAddress>
    {
        BankA2NoOp, BankA3NoOp, TripperPlatformNoOp, CrocomireNoOp, BankA5NoOp, BankA6NoOp, CeresDoorNoOp, BankA7NoOp,
        KraidComponentNoOp, BankA8NoOp, BankA9NoOp, MotherBrainBodyNoOp, BankAaNoOp, ChozoStatueNoOp, BankB3NoOp,
    };
}
