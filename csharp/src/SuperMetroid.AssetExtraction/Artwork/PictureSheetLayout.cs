using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
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
    /// <summary>The pictures in order, each with the frame names that show it.</summary>
    private readonly NamedSheetPicture[] pictures;

    /// <summary>Each picture's top-left corner on the sheet.</summary>
    private readonly Position16<ImagePixels>[] corners;

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
        this.pictures = pictures;
        corners = new Position16<ImagePixels>[this.pictures.Length];
        Size = SheetPacking.Pack([.. this.pictures.Select(picture => picture.Picture.Size)], corners, $"Sheet {sheet}'s {this.pictures.Length} pictures");
    }

    /// <summary>The sheet's width and height.</summary>
    internal Extent16<ImagePixels> Size { get; }

    /// <summary>
    /// Paints the pictures onto <paramref name="planes"/>, which are <see cref="Size"/> and clear, and gives the sheet's
    /// manifest: every frame name with its picture's rectangle, and the colouring's colours and shades.
    /// </summary>
    internal SpriteSheetDocument Paint(SpriteSheetPlanes planes)
    {
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < pictures.Length; index++)
        {
            pictures[index].Picture.Paint(planes, corners[index]);
            SpriteSheetFrameDocument document = pictures[index].Picture.Document(corners[index]);
            foreach (string name in pictures[index].Names)
                frames.Add(name, document);
        }
        return new SpriteSheetDocument
        {
            Version = SpriteSheetFile.Version, Palette = SpriteImageColours.Sprite, Colours = colouring.ColourWords(), PaletteBits = colouring.PaletteBits,
            Frames = frames, Shades = colouring.ShadeDocuments(),
        };
    }
}
