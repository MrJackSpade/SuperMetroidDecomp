using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One painted picture cut from a sheet (#1513): its rectangle's pixels, inks and front-tile marks, with the origin, colour
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

    /// <summary>One where the pixel lies in one of its layer's front tiles, zero elsewhere.</summary>
    private readonly byte[] frontTiles;

    /// <summary>Keeps a picture's planes and frame document fields.</summary>
    private SheetPicture(Extent16<ImagePixels> size, Offset16<ImagePixels> origin, ImageColourGroups colourGroups, ActorDepth? depth,
        RgbaPixel[] pixels, byte[] inks, byte[] frontTiles)
    {
        Size = size;
        Origin = origin;
        ColourGroups = colourGroups;
        Depth = depth;
        this.pixels = pixels;
        this.inks = inks;
        this.frontTiles = frontTiles;
        ImageColourGroups rows = colourGroups;
        foreach (int ink in inks)
        {
            if (ink != 0)
                rows = rows.With(ImageColourGroup.Numbered(ink / GroupColours));
        }
        Rows = rows;
        Content = ContentOf();
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
    /// A hash of everything the picture holds: its size, origin, colour groups, depth and each pixel's colour, ink and
    /// front-tile mark. Pictures with one content show alike in one colouring.
    /// </summary>
    internal string Content { get; }

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
        var frontTiles = new byte[area];
        for (int row = 0; row < size.Y; row++)
        {
            int from = (frame.Corner.Y + row) * planes.Size.X + frame.Corner.X;
            planes.Pixels.Slice(from, size.X).CopyTo(pixels.AsSpan(row * size.X, size.X));
            planes.PaletteIndices.Slice(from, size.X).CopyTo(inks.AsSpan(row * size.X, size.X));
            planes.FrontTiles.Slice(from, size.X).CopyTo(frontTiles.AsSpan(row * size.X, size.X));
        }
        return new SheetPicture(size, frame.Origin, ImageColourGroups.FromManifest(frame.ColourGroups), frame.Depth, pixels, inks, frontTiles);
    }

    /// <summary>Copies the picture onto <paramref name="planes"/> with its rectangle's top-left pixel at <paramref name="corner"/>.</summary>
    internal void Paint(SpriteSheetPlanes planes, Position16<ImagePixels> corner)
    {
        for (int row = 0; row < Size.Y; row++)
        {
            int to = (corner.Y + row) * planes.Size.X + corner.X;
            pixels.AsSpan(row * Size.X, Size.X).CopyTo(planes.Pixels.Slice(to, Size.X));
            inks.AsSpan(row * Size.X, Size.X).CopyTo(planes.PaletteIndices.Slice(to, Size.X));
            frontTiles.AsSpan(row * Size.X, Size.X).CopyTo(planes.FrontTiles.Slice(to, Size.X));
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

    /// <summary>Hashes everything the picture holds into <see cref="Content"/>.</summary>
    private string ContentOf()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Size.X);
            writer.Write(Size.Y);
            writer.Write(Origin.X);
            writer.Write(Origin.Y);
            writer.Write(ColourGroups.ManifestMembers);
            writer.Write(Depth?.ToString() ?? "none");
            writer.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(pixels.AsSpan()));
            writer.Write(inks);
            writer.Write(frontTiles);
        }
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }
}
