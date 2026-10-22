using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.VerificationAssert;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks a picture drawn over characters an encounter uploads (#1392): it paints exactly what its frame's parts paint over
/// the room's live OBJ characters, the path such frames drew through before pre-coloured sheets.
/// </summary>
internal static class UploadedPictureChecks
{
    /// <summary>
    /// Asserts <paramref name="pictures"/> paint exactly what <paramref name="parts"/> paint in the runtime's live colours:
    /// every pixel's colour, palette row and priority, with at least one pixel painted.
    /// </summary>
    internal static void AssertPaintsLikeParts(SuperMetroidRuntime runtime, ImmutableArray<SpriteDraw> pictures,
        ImmutableArray<SpriteDraw> parts, string message)
    {
        AssertTrue(pictures.Length > 0 && pictures.All(draw => draw is ImageSpriteDraw), $"{message}: draws from a sheet");
        var cgram = new SnesCgram();
        for (int colour = 0; colour < 256; colour++)
            cgram.SetColor(colour, runtime.Cgram.Colors[colour]);
        int count = SnesPpuLayout.ScreenSize.X * SnesPpuLayout.ScreenSize.Y;
        var expected = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        var actual = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        SnesObjRenderer.RenderResolved(parts, cgram, expected);
        SnesObjRenderer.RenderResolved(pictures, cgram, actual);
        int painted = 0;
        for (int pixel = 0; pixel < count; pixel++)
        {
            if (expected.Pixels[pixel] != actual.Pixels[pixel] || expected.Priorities[pixel] != actual.Priorities[pixel] ||
                expected.Palettes[pixel] != actual.Palettes[pixel])
                throw new InvalidOperationException($"{message}: the picture differs from the frame over the live characters at screen pixel " +
                    $"({pixel % SnesPpuLayout.ScreenSize.X},{pixel / SnesPpuLayout.ScreenSize.X}): colour {actual.Pixels[pixel]} for {expected.Pixels[pixel]}, " +
                    $"row {actual.Palettes[pixel]} for {expected.Palettes[pixel]}, priority {actual.Priorities[pixel]} for {expected.Priorities[pixel]}.");
            if (expected.Palettes[pixel] != 0)
                painted++;
        }
        AssertTrue(painted > 0, $"{message}: paints pixels");
    }
}
