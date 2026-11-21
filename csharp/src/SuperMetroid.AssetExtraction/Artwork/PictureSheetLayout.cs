using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sprite sheet of pictures already painted (#1513): each picture laid once, packed as <see cref="SheetPacking"/> packs
/// a sheet's pictures, under the frame names that show it, and coloured in one colouring that covers every colour group
/// the pictures use. A picture painted for one sheet and laid on this one is drawn as it was, because the colouring agrees
/// with its own sheet's over those groups.
/// </summary>
internal sealed class PictureSheetLayout
{
    /// <summary>The pictures in order, each with the frame names that show it and the rectangle it is laid in.</summary>
    private readonly LaidSheetPicture[] pictures;

    /// <summary>Each distinct rectangle the pictures paint, in the order a picture first paints it, where it lies on the sheet.</summary>
    private readonly IReadOnlyList<PackedPicture<SheetPicture>> laid;

    /// <summary>The colouring the sheet installs.</summary>
    private readonly SheetPictureColouring colouring;

    /// <summary>Lays out <paramref name="pictures"/> in order, coloured in <paramref name="colouring"/>.</summary>
    /// <param name="pictures">The pictures, each with the frame names that show it; no name shows two of them.</param>
    /// <param name="colouring">The colouring, which covers every colour group each picture uses.</param>
    /// <param name="sheet">Names the sheet in failures.</param>
    /// <exception cref="InvalidDataException">
    /// The sheet holds no picture, a frame name shows two pictures, the colouring leaves out a group a picture uses, or the
    /// pictures do not fit on one image.
    /// </exception>
    internal PictureSheetLayout(NamedSheetPicture[] pictures, SheetPictureColouring colouring, string sheet)
    {
        Ensure.NotNull(pictures);
        this.colouring = Ensure.NotNull(colouring);
        if (pictures.Length == 0)
            throw new InvalidDataException($"Sheet {sheet} holds no picture.");
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (NamedSheetPicture picture in pictures)
        {
            if (picture.Names.Count == 0)
                throw new InvalidDataException($"Sheet {sheet} holds a picture no frame shows.");
            if (picture.Names.FirstOrDefault(name => !names.Add(name)) is { } repeated)
                throw new InvalidDataException($"Sheet {sheet} shows two pictures as frame {repeated}.");
            if (ImageColourGroup.Every.Any(group => picture.Picture.Rows.Contains(group) && !colouring.Groups.Contains(group)))
                throw new InvalidDataException($"Sheet {sheet}'s colouring leaves out a colour group frame {picture.Names[0]} uses.");
        }
        // Pictures painting the same rectangle are laid once there, each frame keeping its own origin, colour groups and
        // depth (#1513).
        var distinct = new Dictionary<string, SheetPicture>(StringComparer.Ordinal);
        foreach (NamedSheetPicture picture in pictures)
            distinct.TryAdd(picture.Picture.Pixels, picture.Picture);
        PackedSheet<SheetPicture> packed = SheetPacking.Pack([.. distinct.Values], picture => picture.Size, $"Sheet {sheet}'s {distinct.Count} pictures");
        Dictionary<string, PackedPicture<SheetPicture>> rectangleOf = packed.Pictures.ToDictionary(placed => placed.Picture.Pixels, StringComparer.Ordinal);
        this.pictures = [.. pictures.Select(picture => new LaidSheetPicture(picture, rectangleOf[picture.Picture.Pixels]))];
        laid = packed.Pictures;
        Size = packed.Size;
    }

    /// <summary>The sheet's width and height.</summary>
    internal Extent16<ImagePixels> Size { get; }

    /// <summary>
    /// Paints the pictures onto <paramref name="planes"/>, which are <see cref="Size"/> and clear, and gives the sheet's
    /// manifest: every frame name with its picture's rectangle, and the colouring's colours and shades.
    /// </summary>
    internal SpriteSheetDocument Paint(SpriteSheetPlanes planes)
    {
        foreach (PackedPicture<SheetPicture> rectangle in laid)
            rectangle.Picture.Paint(planes, rectangle.Corner);
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        foreach (LaidSheetPicture picture in pictures)
        {
            SpriteSheetFrameDocument document = picture.Named.Picture.Document(picture.Rectangle.Corner);
            foreach (string name in picture.Named.Names)
                frames.Add(name, document);
        }
        return new SpriteSheetDocument
        {
            Palette = SpriteImageColours.Sprite, Colours = colouring.Colours(), PaletteBits = colouring.PaletteBits,
            Frames = frames, Shades = colouring.ShadeDocuments(),
        };
    }

    /// <summary>One of the sheet's pictures with the rectangle it is laid in, which pictures painting alike share.</summary>
    /// <param name="Named">The picture with the frame names that show it.</param>
    /// <param name="Rectangle">The rectangle its pixels are painted in.</param>
    private sealed record LaidSheetPicture(NamedSheetPicture Named, PackedPicture<SheetPicture> Rectangle);
}
