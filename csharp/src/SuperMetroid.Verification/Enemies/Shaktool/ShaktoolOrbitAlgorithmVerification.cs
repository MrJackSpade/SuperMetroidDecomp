using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>Compares generated Shaktool orbit math with its retail table and signed 8.8 displacement pairs.</summary>
internal static class ShaktoolOrbitAlgorithmVerification
{
    /// <summary>Checks 320 table words, quantization bounds, 256 displacement pairs, and invalid indexes.</summary>
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
            AssertEqual(original, decimal.Truncate(value - error), $"Orbit lower bound {index}");
            AssertEqual(original, decimal.Truncate(value + error), $"Orbit upper bound {index}");
        }
        for (int angle = 0; angle < 256; angle++)
            AssertEqual(new FixedVector<RoomPixels>(
                    SignedSixteenSixteen.FromRaw(EightEight.FromWhole(Original(angle + 64))),
                    SignedSixteenSixteen.FromRaw(EightEight.FromWhole(Original(angle)))),
                ShaktoolOrbitTables.Displacement(new ByteAngle((byte)angle)), $"Shaktool displacement {angle}");
        foreach (int invalid in new[] { int.MinValue, -1, 320, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ShaktoolOrbitTables.Sample(invalid),
                $"Orbit invalid index {invalid}");
    }
}
