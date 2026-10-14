using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers enemy and enemy-projectile codecs resolve.</summary>
[CpuBusBoundary("catalog: enemy cartridge tables")]
internal static class EnemyCartridgeTables
{
    /// <summary>
    /// $86:8000-$86:FFFF: enemy-projectile instruction lists. A presentation frame's operand address
    /// is the bank-local location of its bank-$8D spritemap pointer word.
    /// </summary>
    internal static readonly CpuBusCell ProjectileInstructionBank = CpuBusCell.CartridgeBytes(
        nameof(ProjectileInstructionBank), 0x868000, 0x8000, "enemy-projectile instruction lists (u16 bank-local operands)");

    /// <summary>
    /// $A6:8000-$A6:FFFF: Ridley's bank. Norfair Ridley's reveal source pointers address RGB5 rows
    /// in this bank.
    /// </summary>
    internal static readonly CpuBusCell RidleyBank = CpuBusCell.CartridgeBytes(nameof(RidleyBank), 0xa68000, 0x8000,
        "Ridley BGR555 color rows (u16 little-endian)");
}
