using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEightBitHalfWaveAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 128; index++)
        {
            byte original = rom.ReadByte(EnemyMathReferenceData.ByteSine + index);
            AssertEqual(original, EnemyTrigonometryTables.EightBitHalfWave(index), $"Original byte sine {index}");
            if (index is 0 or 64) continue;
            decimal value = 256 * EnemyTrigonometryTables.UnitHalfWave(index);
            const decimal error = 256 * 0.0000000000000000001m;
            AssertEqual((decimal)original, decimal.Floor(value - error), $"Byte sine lower bound {index}");
            AssertEqual((decimal)original, decimal.Floor(value + error), $"Byte sine upper bound {index}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 128, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyTrigonometryTables.EightBitHalfWave(invalid),
                $"Byte sine invalid index {invalid}");
    }

    private static void VerifyUnsignedHalfWaveAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 128; index++)
        {
            int address = EnemyMathReferenceData.UnsignedSine + index * 2;
            ushort original = (ushort)(rom.ReadByte(address) | (rom.ReadByte(address + 1) << 8));
            AssertEqual(original, EnemyTrigonometryTables.UnsignedHalfWave(index), $"Original unsigned sine {index}");
            if (index is 0 or 64) continue;
            decimal value = 65535 * EnemyTrigonometryTables.UnitHalfWave(index);
            const decimal error = 65535 * 0.0000000000000000001m;
            AssertEqual((decimal)original, decimal.Floor(value - error), $"Unsigned sine lower bound {index}");
            AssertEqual((decimal)original, decimal.Floor(value + error), $"Unsigned sine upper bound {index}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 128, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyTrigonometryTables.UnsignedHalfWave(invalid),
                $"Unsigned sine invalid index {invalid}");
    }
}
