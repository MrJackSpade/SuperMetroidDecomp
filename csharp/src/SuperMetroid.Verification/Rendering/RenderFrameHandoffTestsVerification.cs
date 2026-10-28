using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Rendering;

/// <summary>Checks display packet identity validation and its one combined brightness.</summary>
internal static class RenderFrameHandoffTestsVerification
{
    /// <summary>Checks combined brightness rendering, repeat rendering and rejection of zero sequence or generation identities.</summary>
    internal static void VerifyRenderFrameHandoff()
    {
        // Two dimmings, 2 and 12 of 15, combine once: 2 * 12 / 15 = 1.6, the nearest level being 2.
        int brightness = DisplayBrightness.Combine(2, 12);
        AssertEqual(2, brightness, "two dimmings combine into one brightness, rounded once");
        var first = new DisplayPacket(new(1, 1, ushort.MaxValue),
            new RgbaPixel(198, 127, 63, 255), brightness);
        AssertEqual(brightness, first.Brightness, "the packet carries the combined brightness");
        RgbaPixel[] pixels = DisplayRendering.Render(first);
        AssertEqual(new RgbaPixel(26, 16, 8, 255), pixels[0], "one brightness pass dims each channel once");
        AssertEqual(pixels[0], pixels[^1], "solid packet covers native frame");
        AssertTrue(pixels.SequenceEqual(RenderForComparison(first)),
            "packet repeat render is observational");
        AssertThrows<ArgumentOutOfRangeException>(() => new DisplayPacket(new(0, 1, 0),
            default(RgbaPixel)), "zero sequence rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new DisplayPacket(new(1, 0, 0),
            default(RgbaPixel)), "zero generation rejected");

        Console.WriteLine("  Render handoff: identity validation and combined brightness agree.");
    }
}
