using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A background tilemap's geometry as BGnSC selects it (#223): one, two (horizontal or vertical) or four
/// 32×32-character screens. Each screen is a contiguous <see cref="ScreenGeometry.TilemapScreenCellCount"/>-word
/// block stored row by row; screens are ordered left to right, then top to bottom.
/// </summary>
[EncodingOwner("background tilemap screen layout")]
public readonly record struct BgTilemapGeometry
{
    /// <summary>Validates a tilemap of 32 or 64 characters on each axis.</summary>
    public BgTilemapGeometry(Extent16<TilemapCells> sizeInCharacters)
    {
        int widthInCharacters = sizeInCharacters.X, heightInCharacters = sizeInCharacters.Y;
        if (widthInCharacters is not (ScreenGeometry.TilemapScreenWidthInTiles or 2 * ScreenGeometry.TilemapScreenWidthInTiles))
            throw new ArgumentOutOfRangeException(nameof(widthInCharacters), widthInCharacters, "A BGSC tilemap is one or two screens wide.");
        if (heightInCharacters is not (ScreenGeometry.TilemapScreenWidthInTiles or 2 * ScreenGeometry.TilemapScreenWidthInTiles))
            throw new ArgumentOutOfRangeException(nameof(heightInCharacters), heightInCharacters, "A BGSC tilemap is one or two screens tall.");
        SizeInCharacters = sizeInCharacters;
    }

    /// <summary>Characters across and down the whole map.</summary>
    public Extent16<TilemapCells> SizeInCharacters { get; }

    /// <summary>Wraps a scrolled horizontal pixel into the map; scrolling past either edge repeats the map.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WrapX(int pixelX) => pixelX & (SizeInCharacters.X * ScreenGeometry.BackgroundTileSizePixels - 1);

    /// <summary>Wraps a scrolled vertical pixel into the map.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WrapY(int pixelY) => pixelY & (SizeInCharacters.Y * ScreenGeometry.BackgroundTileSizePixels - 1);

    /// <summary>
    /// The VRAM word holding the entry for a character position: the screen containing it, then the
    /// entry's row and column within that screen, wrapped onto the VRAM bus.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EntryWordAddress(int tilemapBaseWord, Position16<TilemapCells> character)
    {
        const int screenSize = ScreenGeometry.TilemapScreenWidthInTiles;
        int characterX = character.X, characterY = character.Y;
        int screensPerRow = SizeInCharacters.X / screenSize;
        int screen = characterY / screenSize * screensPerRow + characterX / screenSize;
        int withinScreen = characterY % screenSize * screenSize + characterX % screenSize;
        return SnesVramAddressing.WrapWord(tilemapBaseWord + screen * ScreenGeometry.TilemapScreenCellCount + withinScreen);
    }
}
