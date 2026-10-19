using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks BG2 mosaic source-coordinate grouping against physical scanout and scroll behavior.</summary>
internal static class BackgroundMosaicVerification
{
    /// <summary>Exercises block boundaries, the vertical phase behind the HUD, HDMA scroll changes, and register widths.</summary>
    internal static void VerifyBackgroundMosaicSampling()
    {
        // Explicit boundaries catch quantizing world coordinates or starting the
        // vertical group at HUD row 32 instead of the PPU's physical line one.
        var five = new BackgroundMosaicSampling(5);
        AssertEqual(7, five.SourceX(4, 7), "mosaic quantizes before horizontal scroll");
        AssertEqual(12, five.SourceX(5, 7), "mosaic next horizontal block");
        AssertEqual(38, five.SourceY(32, 7), "mosaic vertical phase continues behind HUD");
        AssertEqual(38, five.SourceY(34, 7), "mosaic last row in vertical group");
        AssertEqual(43, five.SourceY(35, 7), "mosaic next physical vertical group");
        AssertEqual(39, five.SourceY(33, 8), "HDMA changes current scroll inside a mosaic block");
        AssertEqual(42, default(BackgroundMosaicSampling).SourceY(34, 7), "default descriptor disables pixel grouping");
        int cases = 0;
        foreach (int register in Enumerable.Range(0, 256))
            AssertEqual((register & 2) != 0 ? (register >> 4) + 1 : 1,
                BackgroundMosaicSamplingConstants.ForBg2((byte)register).Size, "only BG2 enable selects its mosaic width");
        foreach (int size in Enumerable.Range(1, 16))
        foreach (ushort scroll in new ushort[] { 0, 7, 255, 65535 })
        {
            var mosaic = new BackgroundMosaicSampling(size);
            // Enumerate blocks independently, including the partial last block.
            for (int start = 0; start < 256; start += size)
            for (int offset = 0; offset < size && start + offset < 256; offset++)
            {
                AssertEqual(start + scroll, mosaic.SourceX(start + offset, scroll), "horizontal block source");
                AssertEqual(start + scroll + 1, mosaic.SourceY(start + offset, scroll), "vertical block source before map wrap");
                cases++;
            }
        }
        Console.WriteLine($"  Background mosaic: {cases} source-coordinate cases cover all widths, scroll ordering and frame-start vertical phase.");
    }
}
