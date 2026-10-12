using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Reads the native visual-selector word at <paramref name="operand"/> in enemy code bank
    /// <paramref name="bank"/>, the high byte wrapping within the bank as the instruction interpreter's reads do.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for an enemy code bank's native visual-selector operand words")]
    private static ushort ReadNativeEnemyVisualSelector(ISnesAddressSpace rom, byte bank, ushort operand) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(bank << 16 | operand);
}
