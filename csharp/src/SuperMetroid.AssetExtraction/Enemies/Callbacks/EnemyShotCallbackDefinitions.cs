using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies.Callbacks;

/// <summary>Shot/bomb callback identities, not interchangeable with multibox header shortcuts.</summary>
[EncodingOwner("cartridge addresses of the enemy shot callbacks")]
internal static class EnemyShotCallbackDefinitions
{
    /// <summary>$A2:804C, bank-local RTL used by headers and common hitboxes.</summary>
    public static readonly SnesAddress BankA2NoOp = new(0xa2, 0x804c);
    /// <summary>$A3:804C, bank-local RTL including respawn-placeholder shot AI.</summary>
    public static readonly SnesAddress BankA3NoOp = new(0xa3, 0x804c);
    /// <summary>$A5:804C, bank-local RTL used by boss components.</summary>
    public static readonly SnesAddress BankA5NoOp = new(0xa5, 0x804c);
    /// <summary>$A6:804C, bank-local RTL used by headers and common hitboxes.</summary>
    public static readonly SnesAddress BankA6NoOp = new(0xa6, 0x804c);
    /// <summary>$A6:F920, RTL_A6F920, Ceres door shot callback.</summary>
    public static readonly SnesAddress CeresDoorNoOp = new(0xa6, 0xf920);
    /// <summary>$A7:804C, bank-local RTL used by headers and common hitboxes.</summary>
    public static readonly SnesAddress BankA7NoOp = new(0xa7, 0x804c);
    /// <summary>$A7:94B5, RTL_A794B5, Kraid private header/lint/foot callback; does not skip multibox scanning.</summary>
    public static readonly SnesAddress KraidPrivateNoOp = new(0xa7, 0x94b5);
    /// <summary>$A8:804C, bank-local RTL including Ki-Hunter wings.</summary>
    public static readonly SnesAddress BankA8NoOp = new(0xa8, 0x804c);
    /// <summary>$A9:804C, bank-local RTL including Mother Brain tubes.</summary>
    public static readonly SnesAddress BankA9NoOp = new(0xa9, 0x804c);
    /// <summary>$AA:804C, bank-local RTL including Noob Tube cracks.</summary>
    public static readonly SnesAddress BankAANoOp = new(0xaa, 0x804c);
    /// <summary>$AA:E7DC, RTL_AAE7DC, Chozo statue shot callback.</summary>
    public static readonly SnesAddress ChozoNoOp = new(0xaa, 0xe7dc);
    /// <summary>$B3:804C, bank-local RTL including escape animals.</summary>
    public static readonly SnesAddress BankB3NoOp = new(0xb3, 0x804c);

    /// <summary>The twelve literal-RTL shot callbacks in the inventoried header and extended-hitbox entry points.</summary>
    public static readonly IReadOnlySet<SnesAddress> LiteralNoOps = new HashSet<SnesAddress>
    {
        BankA2NoOp, BankA3NoOp, BankA5NoOp, BankA6NoOp, CeresDoorNoOp, BankA7NoOp, KraidPrivateNoOp, BankA8NoOp,
        BankA9NoOp, BankAANoOp, ChozoNoOp, BankB3NoOp,
    };
}
