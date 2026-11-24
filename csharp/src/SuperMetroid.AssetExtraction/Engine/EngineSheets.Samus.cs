using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What cutting Samus's sheets needs beyond what an enemy's does: frames picked by name and written under names given for them.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>The names of the frames one sheet holds.</summary>
    /// <exception cref="InvalidDataException">The family has no such sheet.</exception>
    internal IReadOnlyCollection<string> FramesOf(string family, string sheet) => Document(family, sheet).Frames.Keys;

    /// <summary>Whether a sheet lists <paramref name="frame"/> as one that shows nothing.</summary>
    internal bool ShowsNothing(string family, string sheet, string frame) =>
        Document(family, sheet).Blank?.Contains(frame) == true;

    /// <summary>The point of a frame that is the place of the thing it shows, from the frame's top-left corner, and the frame's size.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    internal (int OriginX, int OriginY, int Width, int Height) FramePlace(string family, string sheet, string frame) =>
        Document(family, sheet).Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed)
            ? (placed.Origin.X, placed.Origin.Y, placed.Size.X, placed.Size.Y)
            : throw new InvalidDataException($"The sheet {sheet} has no frame {frame}.");

    /// <summary>
    /// Cuts named frames of one sheet onto one canvas of their own, as one of the sheet's shades shows them or, with no
    /// shade named, as they are; each is written under the file name <paramref name="fileName"/> gives for it.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame or shade.</exception>
    internal EngineSheetPictures CutNamed(
        EngineAssetTree tree, string family, string folder, string sheet, IEnumerable<string> frames, string? shade, Func<string, string> fileName)
    {
        _ = Document(family, sheet);
        return CutFrames(tree, family, folder, [.. frames.Select(frame => (frame, sheet))], shade is null ? null : [shade], fileName);
    }

    /// <summary>One sheet's document.</summary>
    private InstalledSheetDocument Document(string family, string sheet) =>
        _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
}
