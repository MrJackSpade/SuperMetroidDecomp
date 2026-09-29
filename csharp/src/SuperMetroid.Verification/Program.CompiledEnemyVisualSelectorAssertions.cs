using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies an installed enemy frame selector against the pinned cartridge.
    /// The caller separately guards live execution so this reference read cannot
    /// conceal a production fallback to ROM.
    /// </summary>
    private static void AssertCompiledEnemyVisualSelector(
        SuperMetroidAddressSpace rom,
        ushort enemyDefinition,
        byte bank,
        ushort operandAddress,
        string context)
    {
        AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                enemyDefinition, operandAddress, out ushort selector),
            $"{context}: compiled visual selector exists");
        int address = (bank << 16) | operandAddress;
        ushort native = (ushort)(rom.ReadCartridgeByte(address) |
            rom.ReadCartridgeByte((bank << 16) | unchecked((ushort)(operandAddress + 1))) << 8);
        AssertEqual(native, selector, $"{context}: compiled visual selector matches cartridge");
    }

    /// <summary>Checks a bank-wide selector shared by more than one enemy definition.</summary>
    private static void AssertCompiledEnemyVisualSelector(
        SuperMetroidAddressSpace rom,
        byte bank,
        ushort operandAddress,
        string context)
    {
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, operandAddress,
                out ushort selector),
            $"{context}: compiled visual selector exists");
        int address = (bank << 16) | operandAddress;
        ushort native = (ushort)(rom.ReadCartridgeByte(address) |
            rom.ReadCartridgeByte((bank << 16) | unchecked((ushort)(operandAddress + 1))) << 8);
        AssertEqual(native, selector, $"{context}: compiled visual selector matches cartridge");
    }
}
