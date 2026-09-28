namespace SuperMetroid.Core.Assets;

/// <summary>Cartridge source ranges for Torizo instruction-owned character uploads.</summary>
internal static class TorizoInstructionTileRomData
{
    /// <summary>$AA:B0A5, shared Bomb/Golden Torizo death and recovery characters.</summary>
    internal const int SharedDeathSource = 0xaab0a5;
    /// <summary>Length of the shared death/recovery tile page.</summary>
    internal const int SharedDeathByteCount = 0x0040;

    /// <summary>$AA:B279, the alternating Bomb Torizo statue-crumble page.</summary>
    internal const int StatueCrumbleSource = 0xaab279;
    /// <summary>Length of the statue-crumble tile page.</summary>
    internal const int StatueCrumbleByteCount = 0x0100;

    /// <summary>$AA:B479, left-facing Bomb Torizo attack/death characters.</summary>
    internal const int LeftAttackSource = 0xaab479;
    /// <summary>Length of the left-facing attack/death tile page.</summary>
    internal const int LeftAttackByteCount = 0x0140;

    /// <summary>$AA:B679, right-facing Bomb Torizo attack/death characters.</summary>
    internal const int RightAttackSource = 0xaab679;
    /// <summary>Length of the right-facing attack/death tile page.</summary>
    internal const int RightAttackByteCount = 0x0140;

    /// <summary>$AF:E200, the initial Golden Torizo character upload.</summary>
    internal const int GoldenAwakeningSource = 0xafe200;
    /// <summary>Length of the Golden Torizo awakening tile page.</summary>
    internal const int GoldenAwakeningByteCount = 0x0600;

    /// <summary>$AF:C800, Golden Torizo's alternate left-side attack characters.</summary>
    internal const int GoldenLeftAttackSource = 0xafc800;
    /// <summary>Length of the Golden Torizo alternate left-side tile page.</summary>
    internal const int GoldenLeftAttackByteCount = 0x0040;

    /// <summary>$AF:CA00, Golden Torizo's alternate right-side attack characters.</summary>
    internal const int GoldenRightAttackSource = 0xafca00;
    /// <summary>Length of the Golden Torizo alternate right-side tile page.</summary>
    internal const int GoldenRightAttackByteCount = 0x0040;
}
