using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>Helpers of <see cref="SnesObjRenderer"/> that only verification reads.</summary>
internal static class SnesObjRendererConstants
{
    /// <summary>
    /// Renders the draws into caller-owned storage, cleared to transparent first. With <paramref name="priority"/>,
    /// a pixel whose frontmost draw has another priority stays transparent rather than showing a draw behind it.
    /// </summary>
    public static void Render(
        Span<Rgba32> output,
        ImmutableArray<SpriteDraw> draws,
        IPaletteColors cgram,
        Extent16<ScreenPixels>? size = null,
        int? priority = null)
    {
        Ensure.NotNull(cgram);
        Extent16<ScreenPixels> frame = SnesObjRenderer.RequireFrame(size);
        Ensure.LengthEqual(output, frame.X * frame.Y);
        output.Clear();
        SnesObjRenderer.Paint(frame, DrawColours.ResolveAll(draws, cgram.Colors), priority, new ObjRasterTargets(output));
    }
}
