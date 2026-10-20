using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the frozen compiled enemy sprite-selector catalog against the pinned cartridge (#1367).</summary>
internal static class CompiledEnemyVisualSelectorsVerification
{
    /// <summary>Checks every checked-in selector directly against the pinned cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled enemy visual selectors")]
    internal static void VerifyCompiledEnemyVisualSelectors()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        // Families that run typed programs (#641) no longer resolve frame operands, so their selectors are gone.
        AssertEqual(4011, CompiledEnemyVisualSelectorsTooling.Count,
            "generated fixed visual-selector count");
        foreach (int index in new[] { int.MinValue, -1, CompiledEnemyVisualSelectorsTooling.Count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CompiledEnemyVisualSelectorsTooling.At(index),
                "Shared selector enumeration bounds");
        int previousAddress = -1;
        for (int index = 0; index < CompiledEnemyVisualSelectorsTooling.Count; index++)
        {
            CompiledEnemyVisualSelector entry = CompiledEnemyVisualSelectorsTooling.At(index);
            AssertTrue(entry.Address > previousAddress,
                "generated visual selectors are distinct and sorted");
            previousAddress = entry.Address;
            ushort native = rom.ReadCartridgeWordFixedBank(entry.Address);
            AssertEqual(native, entry.Pointer,
                $"compiled visual selector ${entry.Address:X6}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(
                    SnesLittleEndian.BankByte(entry.Address),
                    unchecked((ushort)entry.Address), out ushort selected),
                $"compiled visual selector lookup ${entry.Address:X6}");
            AssertEqual(native, selected,
                $"compiled visual selector lookup target ${entry.Address:X6}");
        }
        AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0x80, 0x8000, out _),
            "uncatalogued visual selector is not invented");
        Console.WriteLine(
            $"Compiled enemy visuals: {CompiledEnemyVisualSelectorsTooling.Count:N0} distinct sprite selectors match the cartridge; " +
            "sorted lookup and unknown-key rejection pass.");
    }

    /// <summary>Runs the full compiled selector lookup and cartridge identity verification.</summary>
    [VerificationCommand("--verify-enemy-visual-selectors")]
    internal static int VerifyEnemyVisualSelectorsCommand()
    {
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }
}
