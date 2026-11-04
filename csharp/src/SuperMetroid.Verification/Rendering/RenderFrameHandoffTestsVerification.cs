using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Rendering;

/// <summary>Checks display packet frame-number validation and its one combined brightness.</summary>
internal static class RenderFrameHandoffTestsVerification
{
    /// <summary>Checks combined brightness rendering and rejection of a negative frame number.</summary>
    internal static void VerifyRenderFrameHandoff()
    {
        // Two dimmings, 2 and 12 of 15, combine once: 2 * 12 / 15 = 1.6, the nearest level being 2.
        int brightness = DisplayBrightness.Combine(2, 12);
        AssertEqual(2, brightness, "two dimmings combine into one brightness, rounded once");
        var first = new DisplayPacket(ushort.MaxValue,
            new RgbaPixel(198, 127, 63, 255), brightness);
        AssertEqual(brightness, first.Brightness, "the packet carries the combined brightness");
        RgbaPixel[] pixels = DisplayRendering.Render(first);
        AssertEqual(new RgbaPixel(26, 16, 8, 255), pixels[0], "one brightness pass dims each channel once");
        AssertEqual(pixels[0], pixels[^1], "solid packet covers native frame");
        AssertThrows<ArgumentOutOfRangeException>(() => new DisplayPacket(-1,
            default(RgbaPixel)), "negative frame number rejected");

        Console.WriteLine("  Render handoff: frame-number validation and combined brightness agree.");
    }
}
