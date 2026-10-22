using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One installed pre-coloured room background (#1392): the full picture of a 32-tile-wide tilemap a room shows on BG2,
/// coloured with its tileset's palette over the characters the room loads, and the tilemap words it pictures. A library
/// background pictures the two BG2 pages it loads, the first above the second; the scrolling sky pictures all its rows.
/// </summary>
public sealed class RoomBackgroundImage
{
    /// <summary>Tiles along a tilemap page's side.</summary>
    public const int PageTiles = 32;

    /// <summary>The tilemap pages the image pictures.</summary>
    public const int Pages = 2;

    /// <summary>Tilemap words in a library background's two pages.</summary>
    public const int Words = Pages * PageTiles * PageTiles;

    /// <summary>Pixels along a tilemap page's side.</summary>
    public const int PagePixels = PageTiles * 8;

    /// <summary>The frame of an image's sheet that pictures its whole tilemap.</summary>
    public const string FrameName = "pages";

    /// <summary>Checks the words fill whole pairs of 32-tile rows and the sheet's picture covers them.</summary>
    /// <exception cref="InvalidDataException">The words do not fill pairs of rows or the picture is not their size.</exception>
    public RoomBackgroundImage(string name, SpriteSheet sheet, ImmutableArray<ushort> words)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        SpriteSheetFrame picture = Ensure.NotNull(sheet).Frame(FrameName);
        if (words.IsDefaultOrEmpty || words.Length % (2 * PageTiles) != 0)
            throw new InvalidDataException($"Room background {name} pictures {words.Length} tilemap words, not whole pairs of {PageTiles}-tile rows.");
        if (picture.Size.X != PagePixels || picture.Size.Y != words.Length / PageTiles * 8)
            throw new InvalidDataException($"Room background {name} is {picture.Size.X}x{picture.Size.Y}, not its {words.Length / PageTiles} rows.");
        if (picture.Sheet.Colours != SpriteImageColours.Background)
            throw new InvalidDataException($"Room background {name} is not coloured from the background palette.");
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

    /// <summary>The tilemap words pictured, row by row, 32 to a row.</summary>
    public ImmutableArray<ushort> TilemapWords { get; }

    /// <summary>The image's 16x16 cells: two rows by two columns each.</summary>
    public int CellCount => TilemapWords.Length / RoomBlockPictureNames.Tiles;

    /// <summary>The installed name of the sky <paramref name="variant"/>'s rows in a room of <paramref name="tileset"/>.</summary>
    public static string SkyNameOf(ScrollingSkyChunkTable variant, TilesetDefinition tileset) => $"Sky{variant}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>Checks BG2's tilemap pages show the words a library background's image pictures.</summary>
    /// <exception cref="InvalidDataException">
    /// Artwork bound since shows over a word, or VRAM no longer holds the image's words.
    /// </exception>
    [PpuLayoutBoundary("codec: the BG2 tilemap pages a library background loads")]
    public void AssertLoadedIntoBg2(BgArtworkBindings bindings)
    {
        Ensure.NotNull(bindings);
        if (TilemapWords.Length != Words)
            throw new InvalidOperationException($"Room background {Name} does not picture BG2's two pages.");
        for (int word = 0; word < Words; word++)
        {
            // Artwork bound over the tilemap since the background loaded shows there instead of its words.
            if (bindings.ShowsOver(GameplayVramLayout.Bg2TilemapWord + word, out BgArtworkIdentity over))
                throw new InvalidDataException($"{over.Name} is bound over BG2's tilemap word {word}, which room background {Name} pictures.");
            ushort shownWord = bindings.Vram.ReadWord(GameplayVramLayout.Bg2TilemapWord + word);
            if (shownWord != TilemapWords[word])
                throw new InvalidDataException(
                    $"BG2's tilemap word {word} holds ${shownWord:X4}, not the ${TilemapWords[word]:X4} room background {Name} pictures.");
        }
    }

    /// <summary>Checks <paramref name="words"/> are the words the image pictures, as a sky's rows must be.</summary>
    /// <exception cref="InvalidDataException">The words differ.</exception>
    public void AssertPictures(ReadOnlySpan<ushort> words)
    {
        if (!words.SequenceEqual(TilemapWords.AsSpan()))
            throw new InvalidDataException($"The tilemap shown is not the one room background {Name} pictures.");
    }

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
    /// The 16x16 cells of 32-tile-wide <paramref name="words"/>, row by row, each its four tile words: top left, top right,
    /// bottom left, bottom right.
    /// </summary>
    public static IEnumerable<ushort[]> Cells(IReadOnlyList<ushort> words)
    {
        Ensure.NotNull(words);
        for (int cell = 0; cell < words.Count / RoomBlockPictureNames.Tiles; cell++)
        {
            int first = CellWord(cell);
            yield return [words[first], words[first + 1], words[first + PageTiles], words[first + PageTiles + 1]];
        }
    }

    /// <summary>The first tilemap word of cell <paramref name="cell"/>: sixteen cells to a pair of 32-tile rows.</summary>
    public static int CellWord(int cell) => cell / (PageTiles / 2) * 2 * PageTiles + cell % (PageTiles / 2) * 2;
}
