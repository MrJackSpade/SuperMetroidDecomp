using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Reads the retail selector operands used as the oracle for enemy visual verification.</summary>
internal static class NativeEnemyVisualSelectorsVerification
{
    /// <summary>
    /// Reads the native visual-selector word at <paramref name="operand"/> in enemy code bank
    /// <paramref name="bank"/>, the high byte wrapping within the bank as the instruction interpreter's reads do.
    /// </summary>
    internal static ushort ReadNativeEnemyVisualSelector(ISnesAddressSpace rom, byte bank, ushort operand) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, operand)));
}
