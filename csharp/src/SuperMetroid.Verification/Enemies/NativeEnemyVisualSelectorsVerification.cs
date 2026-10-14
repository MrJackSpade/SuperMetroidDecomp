using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Reads the retail selector operands used as the oracle for enemy visual verification.</summary>
internal static class NativeEnemyVisualSelectorsVerification
{
    /// <summary>
    /// Reads the native visual-selector word at <paramref name="operand"/> in enemy code bank
    /// <paramref name="bank"/>, the high byte wrapping within the bank as the instruction interpreter's reads do.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for an enemy code bank's native visual-selector operand words")]
    internal static ushort ReadNativeEnemyVisualSelector(ISnesAddressSpace rom, byte bank, ushort operand) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, operand)));
}
