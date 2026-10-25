using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Rendering;

/// <summary>Checks display packet identity validation and ordered fade rendering.</summary>
internal static class RenderFrameHandoffTestsVerification
{
    /// <summary>Checks ordered fade rendering, repeat rendering and rejection of zero sequence or generation identities.</summary>
    internal static void VerifyRenderFrameHandoff()
    {
        ImmutableArray<FadeLevel> fades = [new(2), new(12)];
        var first = new DisplayPacket(new(1, 1, ushort.MaxValue),
            new RgbaPixel(198, 127, 63, 255), fades);
        RgbaPixel[] pixels = DisplayRendering.Render(first);
        AssertEqual(new RgbaPixel(20, 12, 6, 255), pixels[0], "ordered fade rounding and owned operations");
        AssertEqual(pixels[0], pixels[^1], "solid packet covers native frame");
        AssertTrue(pixels.SequenceEqual(RenderForComparison(first)),
            "packet repeat render is observational");
        AssertThrows<ArgumentOutOfRangeException>(() => new DisplayPacket(new(0, 1, 0),
            default(RgbaPixel)), "zero sequence rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new DisplayPacket(new(1, 0, 0),
            default(RgbaPixel)), "zero generation rejected");

        Console.WriteLine("  Render handoff: identity validation and ordered fades agree.");
    }
}
