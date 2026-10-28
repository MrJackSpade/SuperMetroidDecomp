using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured gameplay message boxes of one tileset's rooms (#1392): one image of each message's text in
/// each form it takes (each button binding a panel names, each save choice), coloured with the tileset's standard BG3
/// colours and the two colours a message box installs, and the tiles each image pictures.
/// </summary>
public sealed class MessageBoxSheet
{
    /// <summary>Tiles in a row of a message: one row of the BG3 tilemap page a message box is drawn into.</summary>
    private const int RowTiles = ScreenGeometry.TilemapScreenWidthInTiles;

    /// <summary>Each image by the tiles it pictures.</summary>
    private readonly Dictionary<string, SpriteSheetFrame> pictures = new(StringComparer.Ordinal);

    /// <summary>Indexes each of <paramref name="messages"/>' tiles into <paramref name="sheet"/>'s frames.</summary>
    /// <exception cref="InvalidDataException">
    /// The sheet is not coloured as BG3's characters are, an image is not its tiles' size, or two images picture the same
    /// tiles.
    /// </exception>
    public MessageBoxSheet(TilesetDefinition tileset, SpriteSheet sheet, IReadOnlyDictionary<string, ImmutableArray<BackgroundTile>> messages)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Messages = Ensure.NotNull(messages);
        foreach ((string name, ImmutableArray<BackgroundTile> tiles) in messages)
        {
            SpriteSheetFrame frame = sheet.Frame(name);
            if (frame.Sheet.Colours != SpriteImageColours.TwoBppBackground)
                throw new InvalidDataException($"{Name} is not coloured as BG3's characters are.");
            if (tiles.Length % RowTiles != 0 || frame.Size.X != RowTiles * 8 || frame.Size.Y != tiles.Length / RowTiles * 8)
                throw new InvalidDataException($"{Name}'s image {name} is not the size of the {tiles.Length} tiles it pictures.");
            if (!pictures.TryAdd(KeyOf(tiles.AsSpan()), frame))
                throw new InvalidDataException($"{Name} pictures the tiles of {name} twice.");
        }
    }

    /// <summary>The tileset whose rooms show the messages.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The images.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The tiles each image pictures, by image.</summary>
    public IReadOnlyDictionary<string, ImmutableArray<BackgroundTile>> Messages { get; }

    /// <summary>The image picturing <paramref name="tiles"/>.</summary>
    /// <exception cref="InvalidDataException">No image pictures the tiles.</exception>
    public SpriteSheetFrame Picture(ReadOnlySpan<BackgroundTile> tiles) =>
        pictures.TryGetValue(KeyOf(tiles), out SpriteSheetFrame? picture) ? picture
            : throw new InvalidDataException($"{Name} pictures no message of the {tiles.Length} tiles shown.");

    /// <summary>The installed name of the message boxes of <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Messages.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>The lookup key of <paramref name="tiles"/>: each tile's fields, in order.</summary>
    private static string KeyOf(ReadOnlySpan<BackgroundTile> tiles)
    {
        var key = new System.Text.StringBuilder(tiles.Length * 12);
        foreach (BackgroundTile tile in tiles)
        {
            key.Append(tile.Tile).Append('.').Append(tile.ColourVariant).Append('.').Append(tile.FlipHorizontally ? 'h' : '-')
                .Append(tile.FlipVertically ? 'v' : '-').Append(tile.HighPriority ? 'p' : '-').Append(';');
        }
        return key.ToString();
    }
}
