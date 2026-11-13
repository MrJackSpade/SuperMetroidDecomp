using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>Compares generated Shaktool orbit math with its retail table and signed 8.8 displacement pairs.</summary>
internal static class ShaktoolOrbitAlgorithmVerification
{
    /// <summary>Checks 320 table words, quantization bounds, 256 displacement pairs, and invalid indexes.</summary>
    internal static void VerifyShaktoolOrbitAlgorithm(CartridgeImportAddressSpace rom)
    {
        short Original(int index)
        {
            int address = EnemyMathReferenceData.ShaktoolOrbit + 2 * index;
            return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        }
        // The native table's index runs a quarter turn behind the angle it holds: its first 64 words are the negative
        // cosine, so word i holds the angle i + 192 within a turn.
        for (int index = 0; index < 320; index++)
        {
            short original = Original(index);
            TrigonometryAngle held = TrigonometryAngle.Wrapped(index + 192);
            AssertEqual(original, ShaktoolOrbitTables.SampleAt(held), $"Original Shaktool word {index}");
            decimal value = ShaktoolOrbitTables.UnquantizedSampleAt(held);
            const decimal error = 3072 * 0.0000000000000000001m;
            AssertEqual(original, decimal.Truncate(value - error), $"Orbit lower bound {index}");
            AssertEqual(original, decimal.Truncate(value + error), $"Orbit upper bound {index}");
        }
        for (int angle = 0; angle < 256; angle++)
            AssertEqual(new FixedVector<RoomPixels>(
                    SixteenSixteenWords.ToDisplacement(EightEight.FromWhole(Original(angle + 64))),
                    SixteenSixteenWords.ToDisplacement(EightEight.FromWhole(Original(angle)))),
                ShaktoolOrbitTables.Displacement(new TrigonometryAngle((byte)angle)), $"Shaktool displacement {angle}");
    }
}
