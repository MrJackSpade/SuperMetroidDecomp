using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One installed pre-coloured room background (#1392): the full picture of the two BG2 tilemap pages a room state's
/// library background loads, the first page above the second, coloured with its tileset's palette over the characters
/// the room loads, and the tilemap words it pictures.
/// </summary>
public sealed class RoomBackgroundImage
{
    /// <summary>Tiles along a tilemap page's side.</summary>
    public const int PageTiles = 32;

    /// <summary>The tilemap pages the image pictures.</summary>
    public const int Pages = 2;

    /// <summary>Tilemap words in the pages.</summary>
    public const int Words = Pages * PageTiles * PageTiles;

    /// <summary>Pixels along a tilemap page's side.</summary>
    public const int PagePixels = PageTiles * 8;

    /// <summary>The frame of an image's sheet that pictures both pages.</summary>
    public const string FrameName = "pages";

    /// <summary>Checks the sheet's picture covers the pages and the words fill them.</summary>
    /// <exception cref="InvalidDataException">The picture is not the pages' size or the words do not fill them.</exception>
    public RoomBackgroundImage(string name, SpriteSheet sheet, ImmutableArray<ushort> words)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        SpriteSheetFrame picture = Ensure.NotNull(sheet).Frame(FrameName);
        if (picture.Size.X != PagePixels || picture.Size.Y != Pages * PagePixels)
            throw new InvalidDataException($"Room background {name} is {picture.Size.X}x{picture.Size.Y}, not its two pages.");
        if (picture.Sheet.Colours != SpriteImageColours.Background)
            throw new InvalidDataException($"Room background {name} is not coloured from the background palette.");
        if (words.Length != Words)
            throw new InvalidDataException($"Room background {name} pictures {words.Length} tilemap words, not {Words}.");
        Name = name;
        Sheet = sheet;
        Picture = picture;
        TilemapWords = words;
    }

    /// <summary>The background's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The picture of both pages, the first above the second.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The tilemap words pictured, page by page, row by row.</summary>
    public ImmutableArray<ushort> TilemapWords { get; }

    /// <summary>
    /// The installed name of the background <paramref name="program"/> loads in a room of <paramref name="tileset"/>,
    /// entered through <paramref name="door"/> when the program transfers for that door, otherwise through no such door.
    /// </summary>
    public static string NameOf(LibraryBackgroundProgram program, CartridgeDoorHeader? door, TilesetDefinition tileset) =>
        $"{Ensure.NotNull(program).Name}{(door is null ? "" : "." + door.Name)}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>The door <paramref name="program"/>'s transfers depend on when entered through <paramref name="door"/>, or null.</summary>
    public static CartridgeDoorHeader? DoorVariant(LibraryBackgroundProgram program, CartridgeDoorHeader door) =>
        Ensure.NotNull(program).Instructions.Any(instruction => instruction.Command == LibraryBackgroundCommand.TransferForDoor
            && instruction.Door == door) ? door : null;

    /// <summary>
    /// The pages' 16x16 cells, page by page and row by row, each its four tile words: top left, top right, bottom left,
    /// bottom right.
    /// </summary>
    public static IEnumerable<ushort[]> Cells(IReadOnlyList<ushort> words)
    {
        Ensure.NotNull(words);
        for (int cell = 0; cell < Words / RoomBlockPictureNames.Tiles; cell++)
        {
            int first = CellWord(cell);
            yield return [words[first], words[first + 1], words[first + PageTiles], words[first + PageTiles + 1]];
        }
    }

    /// <summary>The first tilemap word of cell <paramref name="cell"/>.</summary>
    public static int CellWord(int cell)
    {
        int cellsPerPage = PageTiles / 2 * (PageTiles / 2);
        int page = cell / cellsPerPage, within = cell % cellsPerPage;
        return page * PageTiles * PageTiles + within / (PageTiles / 2) * 2 * PageTiles + within % (PageTiles / 2) * 2;
    }
}
