using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The warnings a room's escapes can write over it (#1555, #2554), which only a room whose tileset has the warning picture
/// sheet has: Zebes's warning, typed over Mother Brain's room's background, and Ceres's, shown over its foreground. A species
/// that writes a warning receives it from the room, so a room that places one without the sheet is refused as it
/// loads.
/// </summary>
/// <param name="sheet">The room's warning picture sheet.</param>
public sealed partial class RoomWarnings(WrittenPictureSheet sheet)
{
    /// <summary>The warning written at the background's depth.</summary>
    public RoomWarning WithBackground { get; } = new(sheet, RoomWarningDepth.WithBackground);

    /// <summary>The warning written at the foreground's depth.</summary>
    public RoomWarning WithForeground { get; } = new(sheet, RoomWarningDepth.WithForeground);
}
