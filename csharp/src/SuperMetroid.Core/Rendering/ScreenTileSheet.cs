using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// An installed sheet of one frontend background artwork's characters, pre-coloured (#1392): each character under each of
/// the eight palette rows, <see cref="Columns"/> to a row, each palette row's block of characters below the one before,
/// coloured with the standard palette of the screen that shows the artwork. A tilemap word draws its character's tile
/// from its palette row's block, flipped and prioritised as the word says.
/// </summary>
public sealed class ScreenTileSheet
{
    /// <summary>The frame picturing every tile.</summary>
    public const string FrameName = "tiles";

    /// <summary>Characters across a row of the sheet.</summary>
    public const int Columns = 32;

    /// <summary>The palette rows a tilemap word can select.</summary>
    public const int PaletteRows = 8;

    /// <summary>Pixels along a tile's side.</summary>
    public const int TilePixels = 8;

    /// <summary>Checks the sheet holds whole blocks of tiles for each palette row, coloured for <paramref name="depth"/>.</summary>
    /// <exception cref="InvalidDataException">The picture is not whole blocks of tiles, or not coloured for the depth.</exception>
    public ScreenTileSheet(SpriteSheetId id, SpriteSheet sheet, BgBitDepth depth)
    {
        Id = id;
        Depth = depth;
        Picture = Ensure.NotNull(sheet).Frame(FrameName);
        if (Picture.Sheet.Colours != ColoursOf(depth))
            throw new InvalidDataException($"Screen tile sheet {id} is not coloured for {depth} characters.");
        if (Picture.Size.X != Columns * TilePixels || Picture.Size.Y == 0 || Picture.Size.Y % (PaletteRows * TilePixels) != 0)
            throw new InvalidDataException($"Screen tile sheet {id} is {Picture.Size.X}x{Picture.Size.Y}, not whole blocks of tiles for each palette row.");
        BlockRows = Picture.Size.Y / (PaletteRows * TilePixels);
    }

    /// <summary>The installed sheet.</summary>
    public SpriteSheetId Id { get; }

    /// <summary>The bit depth of the artwork's characters.</summary>
    public BgBitDepth Depth { get; }

    /// <summary>The picture of every tile.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>Rows of tiles in one palette row's block.</summary>
    public int BlockRows { get; }

    /// <summary>The characters each palette row's block holds.</summary>
    public int Characters => BlockRows * Columns;

    /// <summary>Pixels from one palette row's block to the next.</summary>
    public int RowStride => BlockRows * TilePixels;

    /// <summary>The top-left pixel, within <see cref="Picture"/>, of <paramref name="character"/> under palette row <paramref name="row"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no such character or row.</exception>
    public Position16<ImagePixels> Corner(int character, int row)
    {
        Ensure.BetweenInclusive(0, Characters - 1, character);
        Ensure.BetweenInclusive(0, PaletteRows - 1, row);
        return CornerOf(character, row, BlockRows);
    }

    /// <summary>The top-left pixel of <paramref name="character"/> under palette row <paramref name="row"/> in a sheet of <paramref name="blockRows"/>-row blocks.</summary>
    public static Position16<ImagePixels> CornerOf(int character, int row, int blockRows) => new(
        (ushort)(character % Columns * TilePixels), (ushort)((row * blockRows + character / Columns) * TilePixels));

    /// <summary>Rows of tiles in each palette row's block of a sheet of <paramref name="characters"/> characters.</summary>
    public static int BlockRowsOf(int characters) => (Ensure.BetweenInclusive(1, int.MaxValue, characters) + Columns - 1) / Columns;

    /// <summary>The VRAM words one character of <paramref name="depth"/> occupies.</summary>
    public static int CharacterWordsOf(BgBitDepth depth) => depth == BgBitDepth.FourBpp ? 16 : 8;

    /// <summary>How a sheet of <paramref name="depth"/> characters is coloured.</summary>
    public static SpriteImageColours ColoursOf(BgBitDepth depth) =>
        depth == BgBitDepth.FourBpp ? SpriteImageColours.Background : SpriteImageColours.TwoBppBackground;
}
