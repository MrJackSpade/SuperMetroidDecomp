using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Reads the native little-endian word at bank-$86 offset <paramref name="address"/>, the
    /// high byte wrapping within the bank as the cartridge's instruction-list reads do.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for bank-$86 enemy-projectile instruction-list words")]
    private static ushort ReadNativeEnemyProjectileProgramWord(ISnesAddressSpace rom, ushort address) =>
        unchecked((ushort)(
            rom.ReadCartridgeByte(EnemyProjectileCodePointers.BankBase | address) |
            rom.ReadCartridgeByte(EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Reads the native byte at bank-$86 offset <paramref name="address"/>.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for bank-$86 enemy-projectile instruction-list bytes")]
    private static byte ReadNativeEnemyProjectileProgramByte(ISnesAddressSpace rom, ushort address) =>
        rom.ReadCartridgeByte(EnemyProjectileCodePointers.BankBase | address);
}
