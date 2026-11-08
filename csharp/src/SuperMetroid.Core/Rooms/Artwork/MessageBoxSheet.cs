using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured gameplay message boxes of one tileset's rooms (#1392, #1466): one image of each form a
/// message takes (each button a panel can name, each save choice), named by the frame name of the form it shows and
/// coloured with the tileset's standard effect overlay colours and the two colours a message box installs.
/// </summary>
public sealed class MessageBoxSheet
{
    /// <summary>Each form's image, by the form.</summary>
    private readonly Dictionary<ShownMessage, SpriteSheetFrame> pictures = [];

    /// <summary>Keeps <paramref name="tileset"/>'s message box images of each of <paramref name="forms"/>.</summary>
    /// <param name="tileset">The tileset whose rooms show the messages.</param>
    /// <param name="sheet">The images.</param>
    /// <param name="forms">Every form the message box can show, each of which the sheet pictures (#1536).</param>
    /// <exception cref="InvalidDataException">The sheet pictures a form under no frame of its name.</exception>
    public MessageBoxSheet(TilesetDefinition tileset, SpriteSheet sheet, IEnumerable<ShownMessage> forms)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        foreach (ShownMessage form in Ensure.NotNull(forms))
            pictures[form] = sheet.Frame(form.FrameName);
    }

    /// <summary>The tileset whose rooms show the messages.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The images.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The image of form <paramref name="shown"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="shown"/> is not a form the message box can show.</exception>
    public SpriteSheetFrame Picture(ShownMessage shown) =>
        pictures.TryGetValue(shown, out SpriteSheetFrame? picture) ? picture
            : throw new InvalidDataException($"{Name} pictures no message form {shown.FrameName}.");

    /// <summary>The installed name of the message boxes of <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Messages.{RoomBlockSheet.NameOf(tileset)}";
}
