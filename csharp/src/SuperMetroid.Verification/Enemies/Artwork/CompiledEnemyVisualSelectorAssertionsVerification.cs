using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Compares compiled enemy visual selectors with pinned cartridge operands in exact-bus verifications.</summary>
internal static class CompiledEnemyVisualSelectorAssertionsVerification
{
    /// <summary>
    /// Verifies an installed enemy frame selector against the pinned cartridge.
    /// The caller separately guards live execution so this reference read cannot
    /// conceal a production fallback to ROM.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for an enemy's compiled visual selector word")]
    internal static void AssertCompiledEnemyVisualSelector(
        SuperMetroidAddressSpace rom,
        EnemyDefinitionId enemyDefinition,
        byte bank,
        ushort operandAddress,
        string context)
    {
        AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                enemyDefinition, operandAddress, out ushort selector),
            $"{context}: compiled visual selector exists");
        int address = ((int)new SnesAddress(bank, operandAddress));
        ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(address);
        AssertEqual(native, selector, $"{context}: compiled visual selector matches cartridge");
    }

    /// <summary>Checks a bank-wide selector shared by more than one enemy definition.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for a bank-wide compiled visual selector word")]
    internal static void AssertCompiledEnemyVisualSelector(
        SuperMetroidAddressSpace rom,
        byte bank,
        ushort operandAddress,
        string context)
    {
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(bank, operandAddress,
                out ushort selector),
            $"{context}: compiled visual selector exists");
        int address = ((int)new SnesAddress(bank, operandAddress));
        ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(address);
        AssertEqual(native, selector, $"{context}: compiled visual selector matches cartridge");
    }
}
