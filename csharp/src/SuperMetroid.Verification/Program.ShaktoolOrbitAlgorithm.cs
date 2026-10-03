using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolOrbitAlgorithm(SuperMetroidAddressSpace rom)
    {
        short Original(int index)
        {
            int address = EnemyMathReferenceData.ShaktoolOrbit + 2 * index;
            return unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        }
        for (int index = 0; index < 320; index++)
        {
            short original = Original(index);
            AssertEqual(original, ShaktoolOrbitTables.Sample(index), $"Original Shaktool word {index}");
            decimal value = ShaktoolOrbitTables.UnquantizedSample(index);
            const decimal error = 3072 * 0.0000000000000000001m;
            AssertEqual((decimal)original, decimal.Truncate(value - error), $"Orbit lower bound {index}");
            AssertEqual((decimal)original, decimal.Truncate(value + error), $"Orbit upper bound {index}");
        }
        for (int angle = 0; angle < 256; angle++)
            AssertEqual((Original(angle + 64) << 8, Original(angle) << 8),
                ShaktoolOrbitTables.Displacement((byte)angle), $"Shaktool displacement {angle}");
        foreach (int invalid in new[] { int.MinValue, -1, 320, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ShaktoolOrbitTables.Sample(invalid),
                $"Orbit invalid index {invalid}");
    }
}
