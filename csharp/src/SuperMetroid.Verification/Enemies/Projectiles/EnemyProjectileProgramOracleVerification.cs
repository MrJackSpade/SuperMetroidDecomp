using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>Reads bank-$86 program bytes from the retail cartridge as a verifier oracle.</summary>
internal static class EnemyProjectileProgramOracleVerification
{
    /// <summary>
    /// Reads the native little-endian word at bank-$86 offset <paramref name="address"/>, the
    /// high byte wrapping within the bank as the cartridge's instruction-list reads do.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for bank-$86 enemy-projectile instruction-list words")]
    internal static ushort ReadNativeEnemyProjectileProgramWord(ISnesAddressSpace rom, ushort address) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);

    /// <summary>Reads the native byte at bank-$86 offset <paramref name="address"/>.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for bank-$86 enemy-projectile instruction-list bytes")]
    internal static byte ReadNativeEnemyProjectileProgramByte(ISnesAddressSpace rom, ushort address) =>
        rom.ReadCartridgeByte(EnemyProjectileCodePointers.BankBase | address);
}
