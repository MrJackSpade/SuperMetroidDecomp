using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

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
            IPaletteColors cgram,
            byte obsel,
            Extent16<ScreenPixels>? size = null,
            int? priority = null)
        {
            Ensure.NotNull(oam);
            Ensure.NotNull(vram);
            Ensure.NotNull(cgram);
            Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;
            var output = new Rgba32[frame.X * frame.Y];
            SnesObjRendererConstants.Render(output, FrontendOamDrawsConstants.Draws(oam, vram, obsel), cgram, frame, priority);
            return output;
        }

        /// <summary>
        /// Resolves the winning OBJ and its BG-relative priority for every output pixel in one
        /// walk of the OAM's draws.
        /// </summary>
        /// <remarks>
        /// Ordinary gameplay needs to interleave four OBJ priority groups with BG1 and BG2.
        /// Rendering one priority plane four times is correct but needlessly decodes every
        /// sprite tile four times and allocates four RGBA canvases. This form performs the
        /// expensive character decode once, stores only the winning color plus its priority,
        /// and lets the PPU compositor place that winner at the appropriate point in its BG
        /// ladder. Empty pixels retain <see cref="SnesObjRenderer.TransparentPriority"/>.
        /// </remarks>
        internal static ResolvedObjFrame RenderResolved(
            OamBuffer oam,
            SnesVram vram,
            IPaletteColors cgram,
            byte obsel,
            Extent16<ScreenPixels>? size = null)
        {
            Ensure.NotNull(oam);
            Ensure.NotNull(vram);
            Ensure.NotNull(cgram);
            Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;

            var pixels = new Rgba32[frame.X * frame.Y];
            var priorities = new byte[pixels.Length];
            SnesObjRenderer.RenderResolved(FrontendOamDrawsConstants.Draws(oam, vram, obsel), cgram, new ObjRasterTargets(pixels, priorities), frame);

            return new ResolvedObjFrame(pixels, priorities);
        }

        /// <summary>Renders the draws over a transparent canvas; color index zero stays transparent.</summary>
        internal static Rgba32[] Render(ImmutableArray<SpriteDraw> draws, IPaletteColors cgram, Extent16<ScreenPixels>? size = null)
        {
            Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;
            var output = new Rgba32[frame.X * frame.Y];
            SnesObjRendererConstants.Render(output, draws, cgram, frame);
            return output;
        }

        /// <summary>Resolves the winning draw and its BG-relative priority for every output pixel.</summary>
        internal static ResolvedObjFrame RenderResolved(ImmutableArray<SpriteDraw> draws, IPaletteColors cgram,
            Extent16<ScreenPixels>? size = null)
        {
            Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;
            var pixels = new Rgba32[frame.X * frame.Y];
            var priorities = new byte[pixels.Length];
            SnesObjRenderer.RenderResolved(draws, cgram, new ObjRasterTargets(pixels, priorities), frame);
            return new ResolvedObjFrame(pixels, priorities);
        }
    }
}
