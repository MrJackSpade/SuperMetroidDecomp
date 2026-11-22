using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The warning an escape writes over its room at one depth (#1555, #2554): what it has written, in the order written, each
/// picture at its place on the screen, and the room's warning picture sheet its pictures show from. Only a room whose
/// tileset has the sheet has its warnings (<see cref="RoomWarnings"/>), so nothing is written where it cannot be shown.
/// </summary>
public sealed partial class RoomWarning
{
    /// <summary>Resolves each written picture's frame in the room's warning picture sheet.</summary>
    private readonly WrittenPictureResolver _pictures;

    /// <summary>A warning at <paramref name="depth"/> whose pictures show from <paramref name="sheet"/>.</summary>
    /// <param name="sheet">The room's warning picture sheet.</param>
    /// <param name="depth">The depth the warning shows at.</param>
    internal RoomWarning(WrittenPictureSheet sheet, RoomWarningDepth depth)
    {
        _pictures = new WrittenPictureResolver(Ensure.NotNull(sheet));
        Depth = depth;
    }

    /// <summary>The depth the warning shows at.</summary>
    public RoomWarningDepth Depth { get; }

    /// <summary>What the warning has written, in the order written; nothing before its escape writes any.</summary>
    public RoomWarningText Text { get; private set; } = RoomWarningText.Empty;

    /// <summary>The room's warning picture sheet.</summary>
    public WrittenPictureSheet Sheet => _pictures.Sheet;

    /// <summary>Writes <paramref name="picture"/> as the next part of the warning.</summary>
    /// <param name="picture">The picture written and where.</param>
    internal void Write(WrittenPicture picture) => Text = Text.With(picture);

    /// <summary>
    /// What the warning shows while the room's inputs take the values <paramref name="appearance"/> reads, or null while it
    /// has written nothing.
    /// </summary>
    /// <param name="appearance">The room's appearance inputs.</param>
    /// <exception cref="InvalidDataException">The sheet pictures a written picture for no such values.</exception>
    public ShownRoomWarning? Shown(RoomAppearanceView appearance) =>
        Text.Pictures.IsEmpty ? null : new ShownRoomWarning(_pictures.Resolve(Text, appearance), Depth);
}
