using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured gameplay message boxes of one tileset's rooms (#1392, #1466): one image of each form a
/// message takes (each button a panel can name, each save choice), named by the frame name of the form it shows and
/// coloured with the tileset's standard BG3 colours and the two colours a message box installs.
/// </summary>
public sealed class MessageBoxSheet
{
    /// <summary>Each form's image, once looked up.</summary>
    private readonly Dictionary<string, SpriteSheetFrame> pictures = new(StringComparer.Ordinal);

    /// <summary>Keeps <paramref name="tileset"/>'s message box images.</summary>
    /// <exception cref="InvalidDataException">The sheet is not coloured as BG3's characters are.</exception>
    public MessageBoxSheet(TilesetDefinition tileset, SpriteSheet sheet)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        if (sheet.Image.Colours != SpriteImageColours.TwoBppBackground)
            throw new InvalidDataException($"{Name} is not coloured as BG3's characters are.");
    }

    /// <summary>The tileset whose rooms show the messages.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The images.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The image of the form named <paramref name="frameName"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet pictures no such form.</exception>
    public SpriteSheetFrame Picture(string frameName)
    {
        if (!pictures.TryGetValue(frameName, out SpriteSheetFrame? picture))
            pictures.Add(frameName, picture = Sheet.Frame(frameName));
        return picture;
    }

    /// <summary>The installed name of the message boxes of <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Messages.{RoomBlockSheet.NameOf(tileset)}";
}
