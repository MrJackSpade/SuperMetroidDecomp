using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="BgCharacterGrid"/> that only verification reads.</summary>
internal static class BgCharacterGridConstants
{
    /// <summary>The source row or column of a character pixel, mirrored when the entry flips that axis.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SourcePixel(int pixel, bool flipped) =>
        flipped ? ScreenGeometry.BackgroundTileSizePixels - 1 - pixel : pixel;
}
