using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="SnesObjRenderer"/> members production does not use.</summary>
internal static class SnesObjRendererAccess
{
    /// <summary>Provides transparent RGBA and priority-resolved OBJ surfaces from finalized OAM.</summary>
    extension(SnesObjRenderer)
    {
        /// <summary>
        /// Renders finalized OAM over a transparent canvas. Color index zero remains
        /// transparent, matching the OBJ-specific transparency rule in the SNES PPU.
        /// </summary>
        internal static Rgba32[] Render(
            OamBuffer oam,
            SnesVram vram,
            SnesCgram cgram,
            byte obsel,
            Extent16<ScreenPixels>? size = null,
            int? priority = null)
        {
            Ensure.NotNull(oam);
            Ensure.NotNull(vram);
            Ensure.NotNull(cgram);
            Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;
            var output = new Rgba32[frame.X * frame.Y];
            SnesObjRenderer.Render(output, oam, vram, cgram, obsel, frame, priority);
            return output;
        }

        /// <summary>
        /// Resolves the winning OBJ and its BG-relative priority for every output pixel in one
        /// OAM walk.
        /// </summary>
        /// <remarks>
        /// Ordinary gameplay needs to interleave four OBJ priority groups with BG1 and BG2.
        /// Calling <see cref="Render"/> four times is correct but needlessly decodes every
        /// sprite tile four times and allocates four RGBA canvases. This form performs the
        /// expensive character decode once, stores only the winning color plus its priority,
        /// and lets the PPU compositor place that winner at the appropriate point in its BG
        /// ladder. Empty pixels retain <see cref="SnesObjRenderer.TransparentPriority"/>.
        /// </remarks>
        internal static ResolvedObjFrame RenderResolved(
            OamBuffer oam,
            SnesVram vram,
            SnesCgram cgram,
            byte obsel,
            Extent16<ScreenPixels>? size = null)
        {
            Ensure.NotNull(oam);
            Ensure.NotNull(vram);
            Ensure.NotNull(cgram);
            Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;

            var pixels = new Rgba32[frame.X * frame.Y];
            var priorities = new byte[pixels.Length];
            SnesObjRenderer.RenderResolved(oam, vram, cgram, obsel, new ObjRasterTargets(pixels, priorities), frame);

            return new ResolvedObjFrame(pixels, priorities);
        }
    }
}
