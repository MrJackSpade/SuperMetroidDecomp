using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>Converts background pixel coordinates to the 8×8 characters that cover them (#223).</summary>
[EncodingOwner("background character grid")]
public static class BgCharacterGrid
{
    /// <summary>The character (tile) column or row containing a non-negative pixel coordinate.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CharacterOf(int pixel) => pixel / ScreenGeometry.BackgroundTileSizePixels;

    /// <summary>The pixel's position within its character, zero through seven.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PixelWithinCharacter(int pixel) => pixel % ScreenGeometry.BackgroundTileSizePixels;
}
