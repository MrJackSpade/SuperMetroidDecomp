using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured room artwork (#1392, #1466): the block sheet of each tileset a room can load with the
/// appearance inputs its blocks depend on, the image of each library background a room state loads, and the sheets of the
/// words written over the rooms' backgrounds.
/// </summary>
public sealed class InstalledRoomArtwork
{
    /// <summary>
    /// Keeps the sheets, each under its own tileset, the background images, each under its own name, and the tile word
    /// sheets, each under its own tileset.
    /// </summary>
    /// <exception cref="ArgumentException">A sheet is filed under another tileset, or an image under another name.</exception>
    public InstalledRoomArtwork(IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> sheets,
        IReadOnlyDictionary<string, RoomBackgroundImage> backgrounds, IReadOnlyDictionary<TilesetDefinition, TileWordSheet> tileWords,
        IReadOnlyDictionary<TilesetDefinition, TileWordSheet> bg3Words, IReadOnlyDictionary<TilesetDefinition, MessageBoxSheet> messageBoxes,
        IReadOnlyDictionary<TilesetDefinition, FloorTileSheet> floorTiles)
    {
        FloorTiles = Ensure.NotNull(floorTiles);
        foreach ((TilesetDefinition tileset, FloorTileSheet tiles) in floorTiles)
        {
            if (tiles.Tileset != tileset)
                throw new ArgumentException($"{tiles.Name} is filed under {tileset}.", nameof(floorTiles));
        }
        MessageBoxes = Ensure.NotNull(messageBoxes);
        foreach ((TilesetDefinition tileset, MessageBoxSheet messages) in messageBoxes)
        {
            if (messages.Tileset != tileset)
                throw new ArgumentException($"{messages.Name} is filed under {tileset}.", nameof(messageBoxes));
        }
        Sheets = Ensure.NotNull(sheets);
        Backgrounds = Ensure.NotNull(backgrounds);
        TileWords = Ensure.NotNull(tileWords);
        Bg3Words = Ensure.NotNull(bg3Words);
        foreach ((TilesetDefinition tileset, TileWordSheet words) in tileWords.Concat(bg3Words))
        {
            if (words.Tileset != tileset)
                throw new ArgumentException($"{words.Name} is filed under {tileset}.", nameof(tileWords));
        }
        if (tileWords.Values.Any(words => words.Layer != TileWordLayer.Bg12) || bg3Words.Values.Any(words => words.Layer != TileWordLayer.Bg3))
            throw new ArgumentException("A tile word sheet is filed under another layer's sheets.", nameof(tileWords));
        foreach ((string name, RoomBackgroundImage background) in backgrounds)
        {
            if (background.Name != name)
                throw new ArgumentException($"Room background {background.Name} is filed under {name}.", nameof(backgrounds));
        }
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in sheets)
        {
            if (sheet.Tileset != tileset)
                throw new ArgumentException($"The block sheet of {sheet.Tileset} is filed under {tileset}.", nameof(sheets));
        }
    }

    /// <summary>The sheets by tileset.</summary>
    public IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> Sheets { get; }

    /// <summary>The library background images by name.</summary>
    public IReadOnlyDictionary<string, RoomBackgroundImage> Backgrounds { get; }

    /// <summary>The sheets of words written over BG1 and BG2, by the tileset whose rooms the words are written in.</summary>
    public IReadOnlyDictionary<TilesetDefinition, TileWordSheet> TileWords { get; }

    /// <summary>The sheets of the HUD's and room effects' BG3 words, by the tileset whose colours they show in.</summary>
    public IReadOnlyDictionary<TilesetDefinition, TileWordSheet> Bg3Words { get; }

    /// <summary>The floor tiles of each Ceres tileset.</summary>
    public IReadOnlyDictionary<TilesetDefinition, FloorTileSheet> FloorTiles { get; }

    /// <summary>The floor tiles of Ceres tileset <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No floor tiles are installed for the tileset.</exception>
    public FloorTileSheet FloorTilesOf(TilesetDefinition tileset) =>
        FloorTiles.TryGetValue(tileset, out FloorTileSheet? tiles) ? tiles : throw new InvalidDataException($"No floor tiles are installed for {tileset}.");

    /// <summary>The message box images of each tileset's rooms.</summary>
    public IReadOnlyDictionary<TilesetDefinition, MessageBoxSheet> MessageBoxes { get; }

    /// <summary>The message box images of <paramref name="tileset"/>'s rooms.</summary>
    /// <exception cref="InvalidDataException">No message box images are installed for the tileset.</exception>
    public MessageBoxSheet MessageBox(TilesetDefinition tileset) =>
        MessageBoxes.TryGetValue(tileset, out MessageBoxSheet? messages) ? messages
            : throw new InvalidDataException($"No message box images are installed for {tileset}.");

    /// <summary>The BG3 word sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public TileWordSheet Bg3(TilesetDefinition tileset) =>
        Bg3Words.TryGetValue(tileset, out TileWordSheet? words) ? words : throw new InvalidDataException($"No BG3 word sheet is installed for {tileset}.");

    /// <summary>A content identity of every sheet, in tileset order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomArtwork), content =>
    {
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
        {
            content.AppendName("tileset", RoomBlockSheet.NameOf(tileset));
            sheet.Sheet.AppendIdentity(content);
            sheet.Inputs.AppendIdentity(content);
        }
        foreach ((string name, RoomBackgroundImage background) in Backgrounds.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            content.AppendName("background", name);
            background.Sheet.AppendIdentity(content);
            foreach (RoomBackgroundCell cell in background.Cells)
            {
                content.Append("cell", cell.Cell);
                content.AppendName("picture", cell.Picture);
                AppendInputs(content, cell.Inputs);
            }
        }
        foreach (FloorTileSheet tiles in FloorTiles.Values.OrderBy(tiles => tiles.Name, StringComparer.Ordinal))
        {
            content.AppendName("mode7", tiles.Name);
            tiles.Sheet.AppendIdentity(content);
            foreach (bool pictured in tiles.Pictured)
                content.AppendName("pictured", pictured ? "yes" : "no");
        }
        foreach (MessageBoxSheet messages in MessageBoxes.Values.OrderBy(messages => messages.Name, StringComparer.Ordinal))
        {
            content.AppendName("messages", messages.Name);
            messages.Sheet.AppendIdentity(content);
            foreach ((string name, ImmutableArray<BackgroundTile> tiles) in messages.Messages.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.AppendName("message", name);
                content.AppendTiles("tiles", tiles.AsSpan());
            }
        }
        foreach (TileWordSheet words in TileWords.Values.Concat(Bg3Words.Values).OrderBy(words => words.Name, StringComparer.Ordinal))
        {
            content.AppendName("words", words.Name);
            words.Sheet.AppendIdentity(content);
            foreach (TileWordSheetTile tile in words.Tiles)
            {
                content.AppendTiles("tile", [tile.Tile.Tile]);
                content.AppendName("appearance", tile.Tile.Appearance);
                content.AppendName("frame", tile.Frame);
                content.Append("x", tile.Corner.X);
                content.Append("y", tile.Corner.Y);
            }
            foreach ((BackgroundTile tile, ImmutableArray<AppearanceInput> inputs) in words.Inputs.OrderBy(entry => entry.Key.Tile)
                .ThenBy(entry => entry.Key.ColourVariant).ThenBy(entry => entry.Key.FlipHorizontally).ThenBy(entry => entry.Key.FlipVertically)
                .ThenBy(entry => entry.Key.HighPriority))
            {
                content.AppendTiles("depends", [tile]);
                AppendInputs(content, inputs);
            }
        }
    });

    /// <summary>Appends the tokens of <paramref name="inputs"/> to a content identity.</summary>
    private static void AppendInputs(SelectedPresentationHash content, ImmutableArray<AppearanceInput> inputs)
    {
        content.Append("inputs", inputs.Length);
        foreach (AppearanceInput input in inputs)
            content.AppendName("input", input.Token);
    }

    /// <summary>The library background image named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">No such image is installed.</exception>
    public RoomBackgroundImage Background(string name) =>
        Backgrounds.TryGetValue(name, out RoomBackgroundImage? background) ? background
            : throw new InvalidDataException($"No room background image {name} is installed.");

    /// <summary>The block sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public RoomBlockSheet Get(TilesetDefinition tileset) =>
        Sheets.TryGetValue(tileset, out RoomBlockSheet? sheet) ? sheet : throw new InvalidDataException($"No block sheet is installed for {tileset}.");
}
