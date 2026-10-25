using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Hardware;

using SuperMetroid.Verification.Enemies;

/// <summary>Compares generated half-wave tables with the retail ROM's byte and word samples.</summary>
internal static class HalfWaveAlgorithmsVerification
{
    /// <summary>Checks all 128 byte samples, precise rounding bounds, and out-of-range indices.</summary>
    internal static void VerifyEightBitHalfWaveAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 128; index++)
        {
            byte original = EightBitHalfWaveAlgorithmRomByte(rom, EnemyMathReferenceData.ByteSine + index);
            AssertEqual(original, EnemyTrigonometryTables.EightBitHalfWave(index), $"Original byte sine {index}");
            if (index is 0 or 64) continue;
            decimal value = 256 * EnemyTrigonometryTables.UnitHalfWave(index);
            const decimal error = 256 * 0.0000000000000000001m;
            AssertEqual(original, decimal.Floor(value - error), $"Byte sine lower bound {index}");
            AssertEqual(original, decimal.Floor(value + error), $"Byte sine upper bound {index}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 128, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyTrigonometryTables.EightBitHalfWave(invalid),
                $"Byte sine invalid index {invalid}");
    }

    /// <summary>Reads the original byte sine table only for this retail oracle check.</summary>
    private static byte EightBitHalfWaveAlgorithmRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks all 128 unsigned word samples, precise rounding bounds, and invalid indices.</summary>
    internal static void VerifyUnsignedHalfWaveAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 128; index++)
        {
            int address = EnemyMathReferenceData.UnsignedSine + index * 2;
            ushort original = SnesLittleEndian.Word(UnsignedHalfWaveAlgorithmRomByte(rom, address), UnsignedHalfWaveAlgorithmRomByte(rom, address + 1));
            AssertEqual(original, EnemyTrigonometryTables.UnsignedHalfWave(index), $"Original unsigned sine {index}");
            if (index is 0 or 64) continue;
            decimal value = 65535 * EnemyTrigonometryTables.UnitHalfWave(index);
            const decimal error = 65535 * 0.0000000000000000001m;
            AssertEqual(original, decimal.Floor(value - error), $"Unsigned sine lower bound {index}");
            AssertEqual(original, decimal.Floor(value + error), $"Unsigned sine upper bound {index}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 128, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyTrigonometryTables.UnsignedHalfWave(invalid),
                $"Unsigned sine invalid index {invalid}");
    }

    /// <summary>Reads bytes of the original unsigned sine table for this retail oracle check.</summary>
    private static byte UnsignedHalfWaveAlgorithmRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
