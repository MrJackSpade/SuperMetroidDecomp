using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One painted picture cut from a sheet (#1513): its rectangle's pixels and inks, with the origin, colour
/// groups and depth its frame document names, so it can be laid on another sheet and drawn as it was. What it shows in each
/// shade is its sheet's colouring over <see cref="Rows"/>, which it does not hold.
/// </summary>
internal sealed class SheetPicture
{
    /// <summary>Colours in one colour group.</summary>
    private const int GroupColours = ImageColourGroup.Colours;

    /// <summary>Each pixel's colour, row by row from the top of the rectangle.</summary>
    private readonly RgbaPixel[] pixels;

    /// <summary>Each pixel's colour group's place times sixteen plus its colour, zero where transparent.</summary>
    private readonly byte[] inks;

    /// <summary>Keeps a picture's planes and frame document fields.</summary>
    private SheetPicture(Extent16<ImagePixels> size, Offset16<ImagePixels> origin, ImageColourGroups colourGroups, ActorDepth? depth,
        RgbaPixel[] pixels, byte[] inks)
    {
        Size = size;
        Origin = origin;
        ColourGroups = colourGroups;
        Depth = depth;
        this.pixels = pixels;
        this.inks = inks;
        ImageColourGroups rows = colourGroups;
        foreach (int ink in inks)
        {
            if (ink != 0)
                rows = rows.With(ImageColourGroup.Numbered(ink / GroupColours));
        }
        Rows = rows;
        Pixels = PixelsOf();
    }

    /// <summary>The rectangle's width and height; zero for an empty picture.</summary>
    internal Extent16<ImagePixels> Size { get; }

    /// <summary>The frame's origin, from the rectangle's top-left pixel.</summary>
    internal Offset16<ImagePixels> Origin { get; }

    /// <summary>The colour groups the frame was coloured from.</summary>
    internal ImageColourGroups ColourGroups { get; }

    /// <summary>Where a gameplay draw of the frame sits among the room's layers; none for an empty frame.</summary>
    internal ActorDepth? Depth { get; }

    /// <summary>
    /// The colour groups whose colours a draw of the picture shows: those its pixels ink and those its frame names. Its
    /// sheet's colours and shades of every other group say nothing about it.
    /// </summary>
    internal ImageColourGroups Rows { get; }

    /// <summary>
    /// A hash of the rectangle alone: its size and each pixel's colour and ink. Pictures with one such
    /// hash are one rectangle of a sheet, whatever origin, colour groups and depth each of their frames names (#1513).
    /// </summary>
    internal string Pixels { get; }

    /// <summary>The picture <paramref name="frame"/> places on <paramref name="planes"/>, copied out of them.</summary>
    /// <exception cref="InvalidDataException">The frame's rectangle leaves the planes.</exception>
    internal static SheetPicture Cut(SpriteSheetPlanes planes, SpriteSheetFrameDocument frame)
    {
        Ensure.NotNull(frame);
        Extent16<ImagePixels> size = frame.Size;
        if (frame.Corner.X + size.X > planes.Size.X || frame.Corner.Y + size.Y > planes.Size.Y)
            throw new InvalidDataException($"A {size.X}x{size.Y} frame at ({frame.Corner.X}, {frame.Corner.Y}) leaves its {planes.Size.X}x{planes.Size.Y} sheet.");
        int area = size.X * size.Y;
        var pixels = new RgbaPixel[area];
        var inks = new byte[area];
        for (int row = 0; row < size.Y; row++)
        {
            int from = (frame.Corner.Y + row) * planes.Size.X + frame.Corner.X;
            planes.Pixels.Slice(from, size.X).CopyTo(pixels.AsSpan(row * size.X, size.X));
            planes.PaletteIndices.Slice(from, size.X).CopyTo(inks.AsSpan(row * size.X, size.X));
        }
        return new SheetPicture(size, frame.Origin, ImageColourGroups.FromManifest(frame.ColourGroups), frame.Depth, pixels, inks);
    }

    /// <summary>Copies the picture onto <paramref name="planes"/> with its rectangle's top-left pixel at <paramref name="corner"/>.</summary>
    internal void Paint(SpriteSheetPlanes planes, Position16<ImagePixels> corner)
    {
        for (int row = 0; row < Size.Y; row++)
        {
            int to = (corner.Y + row) * planes.Size.X + corner.X;
            pixels.AsSpan(row * Size.X, Size.X).CopyTo(planes.Pixels.Slice(to, Size.X));
            inks.AsSpan(row * Size.X, Size.X).CopyTo(planes.PaletteIndices.Slice(to, Size.X));
        }
    }

    /// <summary>The picture's frame document on a sheet that lays it at <paramref name="corner"/>.</summary>
    internal SpriteSheetFrameDocument Document(Position16<ImagePixels> corner) => new()
    {
        Corner = corner,
        Size = Size,
        Origin = Origin,
        ColourGroups = ColourGroups.ManifestMembers,
        Depth = Depth,
    };

    /// <summary>
    /// What a draw of the picture shows in <paramref name="colouring"/>, its sprite sheet's colouring over <see cref="Rows"/>
    /// (#1544): its size, origin and depth, each pixel's colour and how it shows its colour once installed,
    /// the colour it shows named by the order the picture first shows its colours, and the colouring's colours and shades over
    /// those colours. Its palette indices and colour groups are left out, as the installed image names its own colours.
    /// </summary>
    internal SheetPictureLook LookIn(SheetPictureColouring colouring)
    {
        Ensure.NotNull(colouring);
        var shown = new List<int>();
        var orderOf = new Dictionary<int, int>();
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Size.X);
            writer.Write(Size.Y);
            writer.Write(Origin.X);
            writer.Write(Origin.Y);
            writer.Write(Depth?.ToString() ?? "none");
            for (int pixel = 0; pixel < inks.Length; pixel++)
            {
                RgbaPixel colour = pixels[pixel];
                writer.Write(colour.R);
                writer.Write(colour.G);
                writer.Write(colour.B);
                writer.Write(colour.A);
                int ink = inks[pixel];
                if (InstalledSheetColouring.ShownKindOf(SpriteImageColours.Sprite, ink) is not { } kind)
                {
                    writer.Write("transparent");
                    continue;
                }
                writer.Write(kind.ToString());
                if (!orderOf.TryGetValue(ink, out int order))
                {
                    orderOf.Add(ink, order = shown.Count);
                    shown.Add(ink);
                }
                writer.Write(order);
            }
            colouring.WriteShown(writer, shown);
        }
        return new SheetPictureLook(Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length)))), [.. shown]);
    }

    /// <summary>Hashes the rectangle's size and planes into <see cref="Pixels"/>.</summary>
    private string PixelsOf()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Size.X);
            writer.Write(Size.Y);
            writer.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(pixels.AsSpan()));
            writer.Write(inks);
        }
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }
}
