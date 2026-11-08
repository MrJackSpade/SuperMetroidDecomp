using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification's native colour oracle (#1512): a copy of a port picture whose standard colours are the colours its inks
/// show in a native 256-colour image, drawn in <see cref="PictureShading.Standard"/>. Where each ink lies in that image is
/// the import's knowledge (<see cref="InstalledPictureColours"/>); production pictures are only ever shaded.
/// </summary>
internal static class NativeColouredPictures
{
    /// <summary>The colours of a native colour image.</summary>
    internal const int ColorCount = InstalledPictureColoursConstants.CartridgeColourCount;

    /// <summary>
    /// A copy of <paramref name="frame"/>'s sheet whose standard colours are the colours its inks show in
    /// <paramref name="nativeColours"/> under a draw of the frame ORing sprite row <paramref name="drawRow"/> into its parts;
    /// its name, pixels, inks and priorities are kept, so fixtures that pair an installed frame with its composition by name
    /// find the copy's too, and it has no shades. Each colour the frame shows is taken from where the holder drawing the
    /// frame composed it (#1544): a picture held once on a common sheet is sourced as its first holder composed it, and
    /// another holder's colours may lie elsewhere in its colour table.
    /// </summary>
    /// <exception cref="ArgumentException">The image does not hold 256 colours.</exception>
    [WholeImageFixture("a picture coloured from a 256-colour native colour image as a frame's holder composed it")]
    internal static SpriteImage OfHolder(SpriteSheetFrame frame, ReadOnlySpan<RgbColor> nativeColours, int drawRow)
    {
        Ensure.NotNull(frame);
        Ensure.LengthEqual(nativeColours, ColorCount);
        SpriteImage picture = frame.Sheet;
        var standard = new RgbColor[picture.StandardColours.Length];
        for (int ink = 0; ink < standard.Length; ink++)
            standard[ink] = nativeColours[InstalledPictureColoursConstants.CartridgeColourOf(frame, ink, drawRow)];
        ReadOnlySpan<RgbaPixel> pixels = picture.Pixels;
        ReadOnlySpan<SpriteImageInk> inks = picture.Inks;
        var shown = new RgbaPixel[pixels.Length];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            // A transparent pixel stays transparent; every other pixel shows its colour's native colour.
            shown[pixel] = pixels[pixel].A == 0 || inks[pixel].Colour is not int colour ? pixels[pixel]
                : standard[colour].ToRgbaPixel() with { A = pixels[pixel].A };
        }
        SpriteImage copy = SpriteImageFixtures.Copied(picture.Name, picture.Size, shown, inks,
            new PictureColours([.. standard]) { Backdrop = picture.Backdrop });
        InstalledPictureColoursConstants.RecordAs(copy, picture);
        return copy;
    }

    /// <summary>The sprite row <paramref name="draw"/>'s colouring ORs into its parts, or zero for a draw in no frontend colouring.</summary>
    private static int DrawRow(ImageSpriteDraw draw) => DrawRow(draw.Image.Sheet, draw.Shading);

    /// <summary>
    /// The sprite row a draw of <paramref name="picture"/> under <paramref name="shading"/> ORs into its parts: the row of
    /// the frontend colouring the shading names for a sprite picture, zero for any other.
    /// </summary>
    internal static int DrawRow(SpriteImage picture, PictureShading? shading) =>
        InstalledPictureColours.SetsOf(Ensure.NotNull(picture)).Palette == SpriteImageColours.Sprite
            ? SuperMetroid.TestSupport.RepositoryInstallation.Verification.DrawnShadeRows.RowOf(shading) ?? 0
            : 0;

    /// <summary>
    /// <paramref name="frame"/> cut from <paramref name="picture"/>, a native-coloured copy of its sheet, at the frame's depth
    /// (#1514), keeping where the frame's holder composed its colours from when that differs from the sheet's record (#1544).
    /// </summary>
    internal static SpriteSheetFrame Of(SpriteSheetFrame frame, SpriteImage picture)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(picture);
        var copy = new SpriteSheetFrame(picture, frame.Corner, frame.Size, frame.Origin) { Depth = frame.Depth };
        if (InstalledPictureColours.FrameSourceOf(frame) is { } source)
            InstalledPictureColours.RecordFrame(copy, source);
        return copy;
    }

    /// <summary>
    /// Which native-coloured copy <paramref name="draw"/> draws from: its sheet's, coloured where its frame's holder composed
    /// the frame's colours when that differs from the sheet's record (#1544), under the row its colouring ORs in.
    /// </summary>
    private static (SpriteImage Sheet, InstalledFrameSource? Source, int Row) CopyOf(ImageSpriteDraw draw) =>
        (draw.Image.Sheet, InstalledPictureColours.FrameSourceOf(draw.Image), DrawRow(draw));

    /// <summary>
    /// The native 256-colour image <paramref name="picture"/> shows under <paramref name="shading"/>: each of its colours
    /// under the shading at the place the import says it was coloured from; the places no colour names stay black.
    /// </summary>
    internal static RgbColor[] ShownIn(SpriteImage picture, PictureShading shading)
    {
        Ensure.NotNull(picture);
        RgbColor[] inks = Ensure.NotNull(shading).InkColours(picture);
        var image = new RgbColor[ColorCount];
        for (int ink = 0; ink < inks.Length; ink++)
            image[InstalledPictureColoursConstants.CartridgeColourOf(picture, ink)] = inks[ink];
        return image;
    }

    /// <summary><paramref name="frame"/> cut from a copy of its sheet coloured from <paramref name="nativeColours"/>.</summary>
    internal static SpriteSheetFrame Of(SpriteSheetFrame frame, ReadOnlySpan<RgbColor> nativeColours) =>
        Of(frame, OfHolder(frame, nativeColours, drawRow: 0));

    /// <summary>
    /// <paramref name="draws"/>, each drawing its picture's copy coloured from <paramref name="nativeColours"/> in its
    /// standard shading; each sheet is copied once.
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
        // A frontend draw's colouring is the row the cartridge ORs into every part (#1512); each sheet is copied once per row,
        // and once more per row for each frame whose holder composed its colours elsewhere than the sheet's record (#1544).
        var holders = new Dictionary<(SpriteImage Sheet, InstalledFrameSource? Source, int Row), SpriteSheetFrame>();
        foreach (ImageSpriteDraw image in images)
            holders.TryAdd(CopyOf(image), image.Image);
        var copies = new Dictionary<(SpriteImage Sheet, InstalledFrameSource? Source, int Row), SpriteImage>();
        foreach (((SpriteImage Sheet, InstalledFrameSource? Source, int Row) key, SpriteSheetFrame holder) in holders)
        {
            SpriteImage copy = OfHolder(holder, nativeColours, key.Row);
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
}
