using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured room artwork (#1392): the block sheet of each tileset a room can load, and the image of each
/// library background a room state loads.
/// </summary>
public sealed class RoomBlockSheetCatalog
{
    /// <summary>
    /// Keeps the sheets, each under its own tileset, the background images, each under its own name, and the tile word
    /// sheets, each under its own tileset.
    /// </summary>
    /// <exception cref="ArgumentException">A sheet is filed under another tileset, or an image under another name.</exception>
    public RoomBlockSheetCatalog(IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> sheets,
        IReadOnlyDictionary<string, RoomBackgroundImage> backgrounds, IReadOnlyDictionary<TilesetDefinition, TileWordSheet> tileWords,
        IReadOnlyDictionary<TilesetDefinition, TileWordSheet> bg3Words, IReadOnlyDictionary<TilesetDefinition, MessageBoxSheet> messageBoxes)
    {
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
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomBlockSheetCatalog), content =>
    {
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
        {
            content.AppendName("tileset", RoomBlockSheet.NameOf(tileset));
            sheet.Sheet.AppendIdentity(content);
        }
        foreach ((string name, RoomBackgroundImage background) in Backgrounds.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            content.AppendName("background", name);
            background.Sheet.AppendIdentity(content);
            foreach (ushort word in background.TilemapWords)
                content.Append("word", word);
        }
        foreach (MessageBoxSheet messages in MessageBoxes.Values.OrderBy(messages => messages.Name, StringComparer.Ordinal))
        {
            content.AppendName("messages", messages.Name);
            messages.Sheet.AppendIdentity(content);
            foreach ((string name, ImmutableArray<ushort> words) in messages.Messages.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.AppendName("message", name);
                content.AppendWords("words", words.AsSpan());
            }
        }
        foreach (TileWordSheet words in TileWords.Values.Concat(Bg3Words.Values).OrderBy(words => words.Name, StringComparer.Ordinal))
        {
            content.AppendName("words", words.Name);
            words.Sheet.AppendIdentity(content);
            foreach (TileWordSheetTile tile in words.Tiles)
            {
                content.Append("word", tile.Tile.Word.Raw);
                content.AppendName("artwork", tile.Tile.Shown.Artwork.Name);
                content.Append("offset", tile.Tile.Shown.WordOffset);
                content.AppendName("frame", tile.Frame);
                content.Append("x", tile.Corner.X);
                content.Append("y", tile.Corner.Y);
            }
        }
    });

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
