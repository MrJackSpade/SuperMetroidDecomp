using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents shaktool orbit algorithm verification.</summary>
internal static class ShaktoolOrbitAlgorithmVerification
{
    /// <summary>Verifies shaktool orbit algorithm.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Shaktool orbit algorithm")]
    internal static void VerifyShaktoolOrbitAlgorithm(SuperMetroidAddressSpace rom)
    {
        short Original(int index)
        {
            int address = EnemyMathReferenceData.ShaktoolOrbit + 2 * index;
            return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
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
            AssertEqual((SnesEightEight.FromWhole(Original(angle + 64)), SnesEightEight.FromWhole(Original(angle))),
                ShaktoolOrbitTables.Displacement((byte)angle), $"Shaktool displacement {angle}");
        foreach (int invalid in new[] { int.MinValue, -1, 320, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ShaktoolOrbitTables.Sample(invalid),
                $"Orbit invalid index {invalid}");
    }
}
