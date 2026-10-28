using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Compares the bounded Kraid nail contour catalog with the original cartridge words.</summary>
internal static class KraidNailContourMappingsVerification
{
    /// <summary>Checks all left-offset records and every relative Y selection against the native contour walk.</summary>
    internal static void VerifyKraidNailLeftOffsets(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(KraidNailLeftOffsetsRomByte(rom, address), KraidNailLeftOffsetsRomByte(rom, address + 1));
        AssertEqual(6, KraidNailContourDefinitions.RecordCount, "Bounded nail contour record count");
        for (int record = 0; record < 6; record++)
            AssertEqual(Word(0xa7bf1d + 4 * record), KraidNailContourDefinitions.LeftAt(record), "Original nail left word");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContourDefinitions.LeftAt(-1), "Negative nail left record");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContourDefinitions.LeftAt(6), "Past nail left record");
        // Independent original-word walk confirms signed wrapping and bounded overreads.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            int record = 0;
            while (unchecked((short)(Word(0xa7bf1f + 4 * record) - raw)) >= 0)
            {
                record++;
                if (record >= 256) throw new InvalidOperationException("Native nail contour walk did not terminate.");
            }
            AssertTrue(record < 6, "Native nail contour terminates within bounded window");
            AssertEqual(Word(0xa7bf1d + 4 * record), KraidNailContourDefinitions.LeftOffset((ushort)raw),
                "Every relative Y selects the native left offset");
        }
    }

    /// <summary>Reads a source byte for the independent Kraid nail left-offset oracle.</summary>
    private static byte KraidNailLeftOffsetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the six compiled nail top boundaries and rejects out-of-range records.</summary>
    internal static void VerifyKraidNailTopBoundaries(SuperMetroidAddressSpace rom)
    {
        for (int record = 0; record < 6; record++)
        {
            int address = 0xa7bf1f + 4 * record;
            ushort expected = LittleEndianBytes.Word(KraidNailTopBoundariesRomByte(rom, address), KraidNailTopBoundariesRomByte(rom, address + 1));
            AssertEqual(expected, KraidNailContourDefinitions.TopAt(record), "Original nail top boundary");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContourDefinitions.TopAt(-1), "Negative nail top record");
        AssertThrows<ArgumentOutOfRangeException>(() => KraidNailContourDefinitions.TopAt(6), "Past nail top record");
    }

    /// <summary>Reads a source byte for the independent Kraid nail top-boundary oracle.</summary>
    private static byte KraidNailTopBoundariesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
