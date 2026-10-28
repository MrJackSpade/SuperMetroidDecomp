using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks the BG2 mosaic block width a gameplay frame carries to the display renderer.</summary>
internal static class BackgroundMosaicVerification
{
    /// <summary>Exercises the register widths and the default descriptor.</summary>
    internal static void VerifyBackgroundMosaicWidth()
    {
        AssertEqual(1, default(BackgroundPixelation).Size, "default descriptor disables pixel grouping");
        int cases = 0;
        foreach (int register in Enumerable.Range(0, 256))
        {
            AssertEqual((register & 2) != 0 ? (register >> 4) + 1 : 1,
                BackgroundMosaicSamplingConstants.ForBg2((byte)register).Size, "only BG2 enable selects its mosaic width");
            cases++;
        }
        foreach (int size in Enumerable.Range(1, 16))
        {
            AssertEqual(size, new BackgroundPixelation(size).Size, "a descriptor keeps its block width");
            cases++;
        }
        Console.WriteLine($"  Background mosaic: {cases} cases cover every register value and block width.");
    }
}
