using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>Compares Shaktool's authored orbit sine with its retail table and signed 8.8 displacement pairs (#2173).</summary>
internal static class ShaktoolOrbitSineVerification
{
    /// <summary>
    /// Checks that each of the 320 table words equals the authored sine at the angle it holds, and that each of the 256
    /// orbit angles places the segment by the words the game reads for it.
    /// </summary>
    internal static void VerifyShaktoolOrbitSine(CartridgeImportAddressSpace rom)
    {
        short Original(int index)
        {
            int address = EnemyMathReferenceData.ShaktoolOrbit + 2 * index;
            return unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        }
        // The native table's index runs a quarter turn behind the angle it holds: its first 64 words are the negative
        // cosine, so word i holds the angle i + 192 within a turn.
        for (int index = 0; index < 320; index++)
            AssertEqual(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(Original(index)), ShaktoolOrbitDefinitions.OrbitSine.At(TrigonometryAngle.Wrapped(index + 192)), $"Original Shaktool word {index}");
        for (int angle = 0; angle < 256; angle++)
            AssertEqual(new FixedVector<RoomPixels>(
                    SixteenSixteenWords.ToDisplacement(EightEightOracle.FromWhole(Original(angle + 64))),
                    SixteenSixteenWords.ToDisplacement(EightEightOracle.FromWhole(Original(angle)))),
                ShaktoolBehavior.OrbitDisplacement(new TrigonometryAngle((byte)angle)), $"Shaktool displacement {angle}");
    }
}
