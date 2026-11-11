using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Every picture one installation writes to its picture store (#1635): a picture is written once per installation. The
/// store's page writer admits each picture as it writes it to a page; a picture whose content
/// (<see cref="InstalledPictureContent"/>: its pixels, recolour regions and blending, compared pixel by pixel) matches one
/// already written is refused with an error naming both, the sheets and frames that showed them and where. The store
/// shares a picture between the frames that show it before it writes it, so the guard proves no second copy is written.
/// </summary>
internal sealed class SheetPictureRegistry
{
    /// <summary>Every picture written so far, by its content, with where it was first shown.</summary>
    private readonly Dictionary<InstalledPictureContent, WrittenPicture> written = new();

    /// <summary>
    /// Admits <paramref name="picture"/>, shown by frame(s) <paramref name="frames"/> of sheet <paramref name="sheet"/>, as
    /// it is written to a page of the store.
    /// </summary>
    /// <exception cref="InvalidDataException">The same picture was written before.</exception>
    internal void Admit(InstalledPictureContent picture, string sheet, string frames)
    {
        Ensure.NotNull(picture);
        ArgumentException.ThrowIfNullOrWhiteSpace(sheet);
        var shown = new WrittenPicture(sheet, frames, picture.Rectangle);
        if (written.TryAdd(picture, shown))
            return;
        WrittenPicture first = written[picture];
        throw new InvalidDataException(
            $"Sheet {shown.Sheet} shows frame(s) [{shown.Frames}] at {shown.Rectangle}, the same picture, pixel for pixel, as frame(s) " +
            $"[{first.Frames}] of sheet {first.Sheet} at {first.Rectangle}, and both were written to the picture store. A picture " +
            "is written once per installation (#1635): every frame showing it must name the first copy.");
    }

    /// <summary>Where a written picture was first shown.</summary>
    /// <param name="Sheet">The sheet.</param>
    /// <param name="Frames">The frames that show it there.</param>
    /// <param name="Rectangle">Its rectangle on that sheet.</param>
    private sealed record WrittenPicture(string Sheet, string Frames, FrameRectangle Rectangle);
}
