using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed pre-coloured gameplay message boxes of one tileset's rooms (#1392): one image of each message's text in
/// each form it takes (each button binding a panel names, each save choice), coloured with the tileset's standard BG3
/// colours and the two colours a message box installs, and the tilemap words each image pictures.
/// </summary>
public sealed class MessageBoxSheet
{
    /// <summary>Each image by the words it pictures.</summary>
    private readonly Dictionary<string, SpriteSheetFrame> pictures = new(StringComparer.Ordinal);

    /// <summary>Indexes each of <paramref name="messages"/>' words into <paramref name="sheet"/>'s frames.</summary>
    /// <exception cref="InvalidDataException">
    /// The sheet is not coloured as BG3's characters are, an image is not its words' size, or two images picture the same
    /// words.
    /// </exception>
    public MessageBoxSheet(TilesetDefinition tileset, SpriteSheet sheet, IReadOnlyDictionary<string, ImmutableArray<ushort>> messages)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Messages = Ensure.NotNull(messages);
        foreach ((string name, ImmutableArray<ushort> words) in messages)
        {
            SpriteSheetFrame frame = sheet.Frame(name);
            if (frame.Sheet.Colours != SpriteImageColours.TwoBppBackground)
                throw new InvalidDataException($"{Name} is not coloured as BG3's characters are.");
            if (words.Length % GameplayMessageRomData.Layout.TilemapWidth != 0 || frame.Size.X != GameplayMessageRomData.Layout.TilemapWidth * 8
                || frame.Size.Y != words.Length / GameplayMessageRomData.Layout.TilemapWidth * 8)
                throw new InvalidDataException($"{Name}'s image {name} is not the size of the {words.Length} words it pictures.");
            if (!pictures.TryAdd(KeyOf(words.AsSpan()), frame))
                throw new InvalidDataException($"{Name} pictures the words of {name} twice.");
        }
    }

    /// <summary>The tileset whose rooms show the messages.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The images.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The words each image pictures, by image.</summary>
    public IReadOnlyDictionary<string, ImmutableArray<ushort>> Messages { get; }

    /// <summary>The image picturing <paramref name="words"/>.</summary>
    /// <exception cref="InvalidDataException">No image pictures the words.</exception>
    public SpriteSheetFrame Picture(ReadOnlySpan<ushort> words) =>
        pictures.TryGetValue(KeyOf(words), out SpriteSheetFrame? picture) ? picture
            : throw new InvalidDataException($"{Name} pictures no message of the {words.Length} words shown.");

    /// <summary>The installed name of the message boxes of <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Messages.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>The lookup key of <paramref name="words"/>.</summary>
    private static string KeyOf(ReadOnlySpan<ushort> words) =>
        Convert.ToHexString(System.Runtime.InteropServices.MemoryMarshal.AsBytes(words));
}
