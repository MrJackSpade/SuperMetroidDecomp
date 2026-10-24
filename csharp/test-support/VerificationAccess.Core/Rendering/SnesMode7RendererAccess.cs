using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="SnesMode7Renderer"/> members production does not use.</summary>
internal static class SnesMode7RendererAccess
{
    /// <summary>Provides an owned Mode 7 viewport for register-level verification.</summary>
    extension(SnesMode7Renderer)
    {
        /// <summary>
        /// Renders one Mode 7 viewport using the signed 8.8 matrix/register convention.
        /// </summary>
        /// <remarks>
        /// The identity values A=D=$0100 and B=C=0 produce one source pixel per screen pixel.
        /// Outside-map policy defaults to M7SEL=$80: bit 7 disables wrapping and bit 6 leaves
        /// overflow transparent. Character-zero fill is available for the distinct $C0 mode.
        /// Wrapping selects M7SEL bit seven clear and takes precedence over character-zero fill.
        /// Transparent color zero is returned with alpha zero so OBJ/backdrop composition can
        /// retain the normal renderer contract.
        /// </remarks>
        internal static Rgba32[] RenderViewport(
            SnesVram vram,
            SnesCgram cgram,
            AffineTransform registers,
            Extent16<ScreenPixels>? size = null)
        {
            Ensure.NotNull(vram);
            Ensure.NotNull(cgram);
            Extent16<ScreenPixels> viewport = size ?? SnesPpuLayout.ScreenSize;

            var output = new Rgba32[viewport.X * viewport.Y];
            SnesMode7RendererConstants.CompositeViewport(output, vram, cgram, registers, viewport);
            return output;
        }
    }
}
