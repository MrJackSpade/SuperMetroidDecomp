using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

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
        /// shown: each picture read back to the one name in the installed warning picture sheet it shows from whose picture
        /// for those values it is, at the place it shows; nothing while the warning shows nothing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// No installed sheet, or more than one, pictures what the warning shows; or a shown picture is the picture of several
        /// of its sheet's names.
        /// </exception>
        internal RoomWarningText ShownText(RoomAppearanceView appearance)
        {
            if (warning.Shown(appearance) is not { } shown)
                return RoomWarningText.Empty;
            WrittenPictureSheet sheet = SheetShowing(shown.Pictures, appearance);
            RoomWarningText text = RoomWarningText.Empty;
            foreach (PlacedWrittenPicture placed in shown.Pictures)
                text = text.With(new WrittenPicture(NameOf(sheet, placed.Picture, appearance), placed.Corner));
            return text;
        }

        /// <summary>What the warning shows, named, in a room as it has loaded: nothing shown over it, no frame selected, no look shown.</summary>
        internal RoomWarningText ShownText() => warning.ShownText(LoadedRoom);
    }

    /// <summary>A room as it has loaded: nothing shown over it, no animation frame selected and no encounter look shown.</summary>
    private static RoomAppearanceView LoadedRoom => new(new RoomAppearance(), new ArtworkFrameSelections(), new EncounterLooks());

    /// <summary>
    /// The installed warning picture sheet that <paramref name="shown"/> shows from: a room's warning shows from its tileset's
    /// installed sheet, whose pictures are cut from that sheet's own image, so only that sheet pictures every one of them
    /// while the room's inputs take <paramref name="appearance"/>'s values.
    /// </summary>
    /// <exception cref="InvalidOperationException">No installed sheet, or more than one, pictures every shown picture.</exception>
    private static WrittenPictureSheet SheetShowing(ImmutableArray<PlacedWrittenPicture> shown, RoomAppearanceView appearance)
    {
        WrittenPictureSheet[] sheets = [.. InstalledFixtures.RoomAssets.BlockSheets.Written.Values.Where(sheet =>
        {
            HashSet<LayeredPicture> pictures = [.. sheet.inputs.Keys.Select(name => sheet.Picture(name, appearance))];
            return shown.All(placed => pictures.Contains(placed.Picture));
        })];
        return sheets.Length == 1 ? sheets[0]
            : throw new InvalidOperationException(
                $"{sheets.Length} installed warning picture sheets picture what the warning shows: {string.Join(", ", sheets.Select(sheet => sheet.Name))}.");
    }

    /// <summary>The one name in <paramref name="sheet"/> whose picture, while the room's inputs take <paramref name="appearance"/>'s values, is <paramref name="picture"/>.</summary>
    /// <exception cref="InvalidOperationException">No name, or more than one, pictures it.</exception>
    private static string NameOf(WrittenPictureSheet sheet, LayeredPicture picture, RoomAppearanceView appearance)
    {
        string[] names = [.. sheet.inputs.Keys.Where(name => sheet.Picture(name, appearance) == picture).Order(StringComparer.Ordinal)];
        return names.Length == 1 ? names[0]
            : throw new InvalidOperationException($"{sheet.Name} shows the warning's picture as {names.Length} pictures: {string.Join(", ", names)}.");
    }
}
