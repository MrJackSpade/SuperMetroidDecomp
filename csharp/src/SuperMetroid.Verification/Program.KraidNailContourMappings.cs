using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class KraidNailContourMappingsVerification
{
    internal static void VerifyKraidNailLeftOffsets(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(SnesLittleEndian.Word(KraidNailLeftOffsetsRomByte(rom, address), KraidNailLeftOffsetsRomByte(rom, address + 1)));
        AssertEqual(6, KraidNailContour.RecordCount, "Bounded nail contour record count");
        for (int record = 0; record < 6; record++)
            AssertEqual(Word(0xa7bf1d + 4 * record), KraidNailContour.LeftAt(record), "Original nail left word");
        AssertThrows<IndexOutOfRangeException>(() => KraidNailContour.LeftAt(-1), "Negative nail left record");
        AssertThrows<IndexOutOfRangeException>(() => KraidNailContour.LeftAt(6), "Past nail left record");
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
            AssertEqual(Word(0xa7bf1d + 4 * record), KraidNailContour.LeftOffset((ushort)raw),
                "Every relative Y selects the native left offset");
        }
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid nail left offsets")]
    private static byte KraidNailLeftOffsetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    internal static void VerifyKraidNailTopBoundaries(SuperMetroidAddressSpace rom)
    {
        for (int record = 0; record < 6; record++)
        {
            int address = 0xa7bf1f + 4 * record;
            ushort expected = (ushort)(SnesLittleEndian.Word(KraidNailTopBoundariesRomByte(rom, address), KraidNailTopBoundariesRomByte(rom, address + 1)));
            AssertEqual(expected, KraidNailContour.TopAt(record), "Original nail top boundary");
        }
        AssertThrows<IndexOutOfRangeException>(() => KraidNailContour.TopAt(-1), "Negative nail top record");
        AssertThrows<IndexOutOfRangeException>(() => KraidNailContour.TopAt(6), "Past nail top record");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid nail top boundaries")]
    private static byte KraidNailTopBoundariesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
