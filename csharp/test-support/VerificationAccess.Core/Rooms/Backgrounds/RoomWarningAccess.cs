using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Reads what a room's warning shows as the pictures it names (#2554): a check compares the pictures the warning shows, not
/// what it holds, each read back to its name through the room's appearance.
/// </summary>
internal static class RoomWarningAccess
{
    /// <summary>What a warning shows, named.</summary>
    extension(RoomWarning warning)
    {
        /// <summary>
        /// What the warning shows while the room's inputs take the values <paramref name="appearance"/> reads, in the order
        /// shown: each picture read back to the one name in the warning's sheet whose picture for those values it is, at the
        /// place it shows; nothing while the warning shows nothing.
        /// </summary>
        /// <exception cref="InvalidOperationException">A shown picture is no picture of the sheet's, or the picture of several.</exception>
        internal RoomWarningText ShownText(RoomAppearanceView appearance)
        {
            if (warning.Shown(appearance) is not { } shown)
                return RoomWarningText.Empty;
            RoomWarningText text = RoomWarningText.Empty;
            foreach (PlacedWrittenPicture placed in shown.Pictures)
                text = text.With(new WrittenPicture(NameOf(warning.Sheet, placed.Picture, appearance), placed.Corner));
            return text;
        }

        /// <summary>What the warning shows, named, in a room as it has loaded: nothing shown over it, no frame selected, no look shown.</summary>
        internal RoomWarningText ShownText() => warning.ShownText(LoadedRoom);
    }

    /// <summary>A room as it has loaded: nothing shown over it, no animation frame selected and no encounter look shown.</summary>
    private static RoomAppearanceView LoadedRoom => new(new RoomAppearance(), new ArtworkFrameSelections(), new EncounterLooks());
}
