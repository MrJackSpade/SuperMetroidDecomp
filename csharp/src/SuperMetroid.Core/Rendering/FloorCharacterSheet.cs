using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// An installed sheet of one frontend floor artwork's characters, pre-coloured (#1392): each 8x8 character,
/// <see cref="TilesPerRow"/> to a row, coloured from the full palette of the screen that shows it.
/// </summary>
public sealed class FloorCharacterSheet
{
    /// <summary>The frame picturing every character.</summary>
    public const string FrameName = "tiles";

    /// <summary>Characters across a row of the sheet.</summary>
    public const int TilesPerRow = 16;

    /// <summary>Pixels along a character's side.</summary>
    public const int TilePixels = 8;

    /// <summary>Bytes of one character's pixels: one colour index each.</summary>
    public const int TileBytes = TilePixels * TilePixels;

    /// <summary>Checks the sheet holds whole rows of characters coloured from the full palette.</summary>
    /// <exception cref="InvalidDataException">The picture is not whole rows of characters, or not coloured from the full palette.</exception>
    public FloorCharacterSheet(SpriteSheetId id, SpriteSheet sheet)
    {
        Id = id;
        Picture = Ensure.NotNull(sheet).Frame(FrameName);
        if (Picture.Sheet.Colours != SpriteImageColours.FullPalette)
            throw new InvalidDataException($"Floor character sheet {id} is not coloured from the full palette.");
        if (Picture.Size.X != TilesPerRow * TilePixels || Picture.Size.Y == 0 || Picture.Size.Y % TilePixels != 0)
            throw new InvalidDataException($"Floor character sheet {id} is {Picture.Size.X}x{Picture.Size.Y}, not whole rows of characters.");
    }

    /// <summary>The installed sheet.</summary>
    public SpriteSheetId Id { get; }

    /// <summary>The picture of every character.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The characters the sheet holds.</summary>
    public int Tiles => Picture.Size.Y / TilePixels * TilesPerRow;

    /// <summary>The top-left pixel of character <paramref name="tile"/> within <see cref="Picture"/>.</summary>
    public static Position16<ImagePixels> Corner(int tile) =>
        new((ushort)(tile % TilesPerRow * TilePixels), (ushort)(tile / TilesPerRow * TilePixels));

    /// <summary>Rows of characters in a sheet of <paramref name="tiles"/> characters.</summary>
    public static int RowsOf(int tiles) => (Ensure.BetweenInclusive(1, int.MaxValue, tiles) + TilesPerRow - 1) / TilesPerRow;
}
