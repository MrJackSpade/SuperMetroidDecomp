using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Checks the six native left-offset words and verifies signed-wrap record selection for every relative Y value.
    /// </summary>
    /// <param name="rom">The address space containing Kraid's original nail-contour table.</param>
    private static void VerifyKraidNailLeftOffsets(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
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

    /// <summary>
    /// Checks all six native top-boundary words and rejects record indices outside the contour table.
    /// </summary>
    /// <param name="rom">The address space containing Kraid's original nail-contour table.</param>
    private static void VerifyKraidNailTopBoundaries(SuperMetroidAddressSpace rom)
    {
        for (int record = 0; record < 6; record++)
        {
            int address = 0xa7bf1f + 4 * record;
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, KraidNailContour.TopAt(record), "Original nail top boundary");
        }
        AssertThrows<IndexOutOfRangeException>(() => KraidNailContour.TopAt(-1), "Negative nail top record");
        AssertThrows<IndexOutOfRangeException>(() => KraidNailContour.TopAt(6), "Past nail top record");
    }
}
