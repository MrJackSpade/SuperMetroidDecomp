using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Every picture one installation writes to its picture store (#1635, #1644): a picture is written once per installation,
/// by its pixels alone. The store's page writer admits each picture as it writes it to a page; a picture whose pixels
/// (<see cref="InstalledPictureContent"/>, compared pixel by pixel) match one already written is refused with an error naming
/// both, the sheets and frames that showed them and where, whatever maps each was shown with: those are the frames' own, and
/// a frame showing written pixels with other maps names the written picture and its own maps. The store shares a picture
/// between the frames that show it before it writes it, so the guard proves no second copy is written.
/// </summary>
internal sealed class SheetPictureRegistry
{
    /// <summary>Every picture written so far, by its pixels, with where it was first shown.</summary>
    private readonly Dictionary<InstalledPictureContent, WrittenPicture> written = new();

    /// <summary>
    /// Admits <paramref name="picture"/>, shown by frame(s) <paramref name="frames"/> of sheet <paramref name="sheet"/>, as
    /// it is written to a page of the store.
    /// </summary>
    /// <exception cref="InvalidDataException">The same pixels were written before.</exception>
    internal void Admit(InstalledPictureContent picture, string sheet, string frames)
    {
        Ensure.NotNull(picture);
        ArgumentException.ThrowIfNullOrWhiteSpace(sheet);
        var shown = new WrittenPicture(sheet, frames, picture.Rectangle);
        if (written.TryAdd(picture, shown))
            return;
        WrittenPicture first = written[picture];
        throw new InvalidDataException(
            $"Sheet {shown.Sheet} shows frame(s) [{shown.Frames}] at {shown.Rectangle}, the same pixels, pixel for pixel, as frame(s) " +
            $"[{first.Frames}] of sheet {first.Sheet} at {first.Rectangle}, and both were written to the picture store. A picture " +
            "is written once per installation (#1635, #1644): every frame showing its pixels must name the first copy, with the " +
            "maps it shows it with.");
    }

    /// <summary>Where a written picture was first shown.</summary>
    /// <param name="Sheet">The sheet.</param>
    /// <param name="Frames">The frames that show it there.</param>
    /// <param name="Rectangle">Its rectangle on that sheet.</param>
    private sealed record WrittenPicture(string Sheet, string Frames, FrameRectangle Rectangle);
}
