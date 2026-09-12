namespace SuperMetroid.Core.Game;

/// <summary>Shot/bomb callback identities, not interchangeable with multibox header shortcuts.</summary>
public static class EnemyShotCallbackDefinitions
{
    /// <summary>$A2:804C, bank-local RTL used by headers and common hitboxes.</summary>
    public const int BankA2NoOp = 0xa2804c;
    /// <summary>$A3:804C, bank-local RTL including respawn-placeholder shot AI.</summary>
    public const int BankA3NoOp = 0xa3804c;
    /// <summary>$A5:804C, bank-local RTL used by boss components.</summary>
    public const int BankA5NoOp = 0xa5804c;
    /// <summary>$A6:804C, bank-local RTL used by headers and common hitboxes.</summary>
    public const int BankA6NoOp = 0xa6804c;
    /// <summary>$A6:F920, RTL_A6F920, Ceres door shot callback.</summary>
    public const int CeresDoorNoOp = 0xa6f920;
    /// <summary>$A7:804C, bank-local RTL used by headers and common hitboxes.</summary>
    public const int BankA7NoOp = 0xa7804c;
    /// <summary>$A7:94B5, RTL_A794B5, Kraid private header/lint/foot callback; does not skip multibox scanning.</summary>
    public const int KraidPrivateNoOp = 0xa794b5;
    /// <summary>$A8:804C, bank-local RTL including Ki-Hunter wings.</summary>
    public const int BankA8NoOp = 0xa8804c;
    /// <summary>$A9:804C, bank-local RTL including Mother Brain tubes.</summary>
    public const int BankA9NoOp = 0xa9804c;
    /// <summary>$AA:804C, bank-local RTL including Noob Tube cracks.</summary>
    public const int BankAANoOp = 0xaa804c;
    /// <summary>$AA:E7DC, RTL_AAE7DC, Chozo statue shot callback.</summary>
    public const int ChozoNoOp = 0xaae7dc;
    /// <summary>$B3:804C, bank-local RTL including escape animals.</summary>
    public const int BankB3NoOp = 0xb3804c;

    /// <summary>Classifies the inventoried header and extended-hitbox shot entry points.</summary>
    public static bool IsLiteralNoOp(byte bank, ushort pointer) => ((bank << 16) | pointer) is
        BankA2NoOp or BankA3NoOp or BankA5NoOp or BankA6NoOp or CeresDoorNoOp or
        BankA7NoOp or KraidPrivateNoOp or BankA8NoOp or BankA9NoOp or BankAANoOp or
        ChozoNoOp or BankB3NoOp;
}
