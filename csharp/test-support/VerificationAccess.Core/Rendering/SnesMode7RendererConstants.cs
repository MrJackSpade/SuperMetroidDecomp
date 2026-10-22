using SuperMetroid.Core.Hardware;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="SnesMode7Renderer"/> that only verification reads.</summary>
internal static class SnesMode7RendererConstants
{
    /// <summary>
    /// Projects Mode 7 directly over caller-owned storage. Transparent samples leave the
    /// existing destination intact, allowing the gameplay compositor to begin with its
    /// backdrop and avoid a second full-frame raster plus copy.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void CompositeViewport(Span<Rgba32> output, SnesVram vram, IPaletteColors cgram, Mode7RenderRegisters registers, Extent16<ScreenPixels>? size = null) =>
        SnesMode7Renderer.CompositeViewport(output, vram, cgram, registers, map: null, tiles: null, size);
}
