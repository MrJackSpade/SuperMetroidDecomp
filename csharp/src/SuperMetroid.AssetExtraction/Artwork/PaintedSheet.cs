using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A laid-out sprite sheet painted and cut into its pictures (#1513): each frame's picture by frame name, frames sharing a
/// rectangle sharing a picture, and the manifest whose colouring the pictures are drawn in. Its pictures can be laid on other
/// sheets of a colouring that agrees with its own over the colour groups they use.
/// </summary>
internal sealed class PaintedSheet
{
    /// <summary>Keeps a painted sheet's manifest and pictures.</summary>
    private PaintedSheet(SpriteSheetDocument document, Dictionary<string, SheetPicture> frames)
    {
        Document = document;
        Frames = frames;
    }

    /// <summary>The painted sheet's manifest, which holds the colouring its pictures are drawn in.</summary>
    internal SpriteSheetDocument Document { get; }

    /// <summary>Each frame's picture, by frame name, in the manifest's order.</summary>
    internal Dictionary<string, SheetPicture> Frames { get; }

    /// <summary>Paints <paramref name="layout"/> on <paramref name="canvas"/> and cuts each of its frames' pictures out.</summary>
    /// <exception cref="InvalidDataException">A frame's rectangle leaves the sheet.</exception>
    internal static PaintedSheet Paint(SpriteSheetLayout layout, SpriteSheetCanvas canvas)
    {
        Ensure.NotNull(layout);
        Ensure.NotNull(canvas);
        SpriteSheetPlanes planes = canvas.Cleared(layout.Size);
        SpriteSheetDocument document = layout.Paint(planes);
        // Frames the layout lays once name one frame document, so they share one picture.
        var pictureOf = new Dictionary<SpriteSheetFrameDocument, SheetPicture>(ReferenceEqualityComparer.Instance);
        var frames = new Dictionary<string, SheetPicture>(StringComparer.Ordinal);
        foreach ((string name, SpriteSheetFrameDocument frame) in document.Frames)
        {
            if (!pictureOf.TryGetValue(frame, out SheetPicture? picture))
                pictureOf.Add(frame, picture = SheetPicture.Cut(planes, frame));
            frames.Add(name, picture);
        }
        return new PaintedSheet(document, frames);
    }
}
