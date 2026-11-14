using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification's native colour oracle (#1512, #1633): a copy of a port picture whose pixels show the colours they show in a
/// native 256-colour image, drawn in <see cref="PictureShading.Standard"/>. Where each pixel's colour lies in that image is
/// the import's knowledge, which a check records for the pictures it composes (<see cref="ComposedPictures"/>); production
/// pictures are only ever shaded.
/// </summary>
internal static class NativeColouredPictures
{
    /// <summary>The colours of a native colour image.</summary>
    internal const int ColorCount = InstalledPictureColoursConstants.CartridgeColourCount;

    /// <summary>
    /// A copy of <paramref name="picture"/> whose pixels show the colours they show in <paramref name="nativeColours"/> under a
    /// draw ORing sprite row <paramref name="drawRow"/> into its parts; its name, size, pixel places and blend map are kept,
    /// so fixtures that pair an installed frame with its composition by name find the copy's too, and it has no shades. A
    /// clear pixel stays as it is stored (#1633); every visible pixel shows the native colour of its composed palette
    /// index, with its own alpha.
    /// </summary>
    /// <exception cref="ArgumentException">The image does not hold 256 colours.</exception>
    /// <exception cref="InvalidOperationException">The picture was not composed or recorded by a check.</exception>
    /// <remarks>
    /// A picture coloured again from the same colours under the same row is the copy coloured last (#1757): the copy is a
    /// function of the picture, the colours and the row, and a check comparing many frames colours the same pictures again.
    /// </remarks>
    [WholeImageFixture("a picture coloured from a 256-colour native colour image")]
    internal static SpriteImage OfSheet(SpriteImage picture, ReadOnlySpan<RgbColor> nativeColours, int drawRow)
    {
        Ensure.NotNull(picture);
        Ensure.LengthEqual(nativeColours, ColorCount);
        Dictionary<int, ColouredCopy> copies = lastCopies.GetOrCreateValue(picture);
        lock (copies)
        {
            if (copies.TryGetValue(drawRow, out ColouredCopy last) && nativeColours.SequenceEqual(last.Colours))
                return last.Copy;
        }
        SpriteImage copy = Coloured(picture, nativeColours, drawRow);
        lock (copies)
            copies[drawRow] = new ColouredCopy(nativeColours.ToArray(), copy);
        return copy;
    }

    /// <summary>Each picture's copy coloured last under each draw row, with the colours it was coloured from.</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SpriteImage, Dictionary<int, ColouredCopy>> lastCopies = [];

    /// <summary>A picture's copy coloured from <paramref name="Colours"/>.</summary>
    /// <param name="Colours">The native colour image the copy was coloured from.</param>
    /// <param name="Copy">The coloured copy.</param>
    private readonly record struct ColouredCopy(RgbColor[] Colours, SpriteImage Copy);

    /// <summary>A new copy of <paramref name="picture"/> coloured as <see cref="OfSheet"/> describes.</summary>
    private static SpriteImage Coloured(SpriteImage picture, ReadOnlySpan<RgbColor> nativeColours, int drawRow)
    {
        ComposedPictureSource source = ComposedPictures.SourceOf(picture);
        ReadOnlySpan<RgbaPixel> pixels = picture.Pixels;
        var shown = new RgbaPixel[pixels.Length];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            shown[pixel] = pixels[pixel].A == 0 ? pixels[pixel]
                : nativeColours[InstalledPictureColoursConstants.CartridgeColourOfIndex(picture, source.PaletteIndices[pixel], drawRow)].ToRgbaPixel()
                    with { A = pixels[pixel].A };
        }
        ColourKey? backdrop = source.Composed.Backdrop is { } backdropIndex
            ? ColourKeyConstants.Of(nativeColours[InstalledPictureColoursConstants.CartridgeColourOfIndex(picture, backdropIndex, drawRow)])
            : null;
        bool[]? blending = picture.Blending.IsEmpty ? null : picture.Blending.ToArray();
        SpriteImage copy = SpriteImageFixtures.Copied(picture.Name, picture.Size, new SpriteImagePixels(shown, RecolourRegions: null, blending),
            new PictureColours { Backdrop = backdrop });
        ComposedPictures.RecordAs(copy, picture);
        return copy;
    }

    /// <summary>The sprite row <paramref name="draw"/>'s colouring ORs into its parts, or zero for a draw in no frontend colouring.</summary>
    private static int DrawRow(ImageSpriteDraw draw) => DrawRow(draw.Image.Sheet, draw.Shading);

    /// <summary>
    /// The sprite row a draw of <paramref name="picture"/> under <paramref name="shading"/> ORs into its parts: the row of
    /// the frontend colouring the shading names for a sprite picture, zero for any other.
    /// </summary>
    internal static int DrawRow(SpriteImage picture, PictureShading? shading) =>
        ComposedPictures.SourceOf(Ensure.NotNull(picture)).Palette == SpriteImageColours.Sprite
            ? SuperMetroid.TestSupport.RepositoryInstallation.Verification.DrawnShadeRows.RowOf(shading) ?? 0
            : 0;

    /// <summary>
    /// <paramref name="frame"/> cut from <paramref name="picture"/>, a native-coloured copy of its sheet, at the frame's depth
    /// (#1514) and blending as the frame does (#1633).
    /// </summary>
    internal static SpriteSheetFrame Of(SpriteSheetFrame frame, SpriteImage picture)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(picture);
        return new SpriteSheetFrame(picture, frame.Corner, frame.Size, frame.Origin) { Depth = frame.Depth, Blends = frame.Blends };
    }

    /// <summary>Which native-coloured copy <paramref name="draw"/> draws from: its sheet's, under the row its colouring ORs in.</summary>
    private static SheetCopy CopyOf(ImageSpriteDraw draw) => new(draw.Image.Sheet, DrawRow(draw));

    /// <summary>
    /// The native 256-colour image <paramref name="picture"/> shows under <paramref name="shading"/>: each of its colour keys
    /// under the shading at every place the import says it was coloured from; the places no key names stay black.
    /// </summary>
    internal static RgbColor[] ShownIn(SpriteImage picture, PictureShading shading)
    {
        Ensure.NotNull(picture);
        RgbColor[] shown = Ensure.NotNull(shading).KeyColours(picture);
        ImmutableArray<ColourKey> keys = picture.ColourKeys;
        var image = new RgbColor[ColorCount];
        for (int key = 0; key < shown.Length; key++)
        {
            foreach (int place in InstalledPictureColoursConstants.CartridgeColoursOf(picture, keys[key]))
                image[place] = shown[key];
        }
        return image;
    }

    /// <summary><paramref name="frame"/> cut from a copy of its sheet coloured from <paramref name="nativeColours"/>.</summary>
    internal static SpriteSheetFrame Of(SpriteSheetFrame frame, ReadOnlySpan<RgbColor> nativeColours) =>
        Of(frame, OfSheet(Ensure.NotNull(frame).Sheet, nativeColours, drawRow: 0));

    /// <summary>
    /// <paramref name="draws"/>, each drawing its picture's copy coloured from <paramref name="nativeColours"/> in its
    /// standard shading; each sheet is copied once per draw row.
    /// </summary>
    /// <exception cref="ArgumentException">A draw is missing or does not draw a picture.</exception>
    internal static ImmutableArray<SpriteDraw> Draws(ImmutableArray<SpriteDraw> draws, ReadOnlySpan<RgbColor> nativeColours)
    {
        if (draws.IsDefault)
            throw new ArgumentException("A frame requires a draw list.", nameof(draws));
        var images = new List<ImageSpriteDraw>(draws.Length);
        foreach (SpriteDraw draw in draws)
        {
            images.Add(draw as ImageSpriteDraw
                ?? throw new ArgumentException($"Sprite draw {draw} draws no picture.", nameof(draws)));
        }
        // A frontend draw's colouring is the row the cartridge ORs into every part (#1512); each sheet is copied once per row.
        var copies = new Dictionary<SheetCopy, SpriteImage>();
        foreach (SheetCopy key in images.Select(CopyOf).Distinct())
        {
            SpriteImage copy = OfSheet(key.Sheet, nativeColours, key.Row);
            copies.Add(key, copy);
        }
        var shown = ImmutableArray.CreateBuilder<SpriteDraw>(draws.Length);
        foreach (ImageSpriteDraw image in images)
        {
            shown.Add(new ImageSpriteDraw(image.Source, Of(image.Image, copies[CopyOf(image)]), image.Origin, image.Mirror)
            {
                Shading = PictureShading.Standard,
                CorpseRot = image.CorpseRot,
            });
        }
        return shown.MoveToImmutable();
    }

    /// <summary>One native-coloured copy of a sheet.</summary>
    /// <param name="Sheet">The sheet copied.</param>
    /// <param name="Row">The sprite row its draws OR into every part.</param>
    private readonly record struct SheetCopy(SpriteImage Sheet, int Row);
}
