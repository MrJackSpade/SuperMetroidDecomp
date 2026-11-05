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
    /// A copy of <paramref name="picture"/> whose standard colours are the colours its inks show in
    /// <paramref name="nativeColours"/>; its name, pixels, inks and priorities are kept, so fixtures that pair
    /// an installed frame with its composition by name find the copy's too, and it has no shades.
    /// </summary>
    /// <exception cref="ArgumentException">The image does not hold 256 colours.</exception>
    [WholeImageFixture("a picture coloured from a 256-colour native colour image")]
    internal static SpriteImage Of(SpriteImage picture, ReadOnlySpan<RgbColor> nativeColours) => Of(picture, nativeColours, drawRow: 0);

    /// <summary>A copy of <paramref name="picture"/> coloured as <see cref="Of(SpriteImage, ReadOnlySpan{RgbColor})"/> colours it, under a draw ORing sprite row <paramref name="drawRow"/> into its parts.</summary>
    [WholeImageFixture("a picture coloured from a 256-colour native colour image under a draw's row")]
    internal static SpriteImage Of(SpriteImage picture, ReadOnlySpan<RgbColor> nativeColours, int drawRow)
    {
        Ensure.NotNull(picture);
        Ensure.LengthEqual(nativeColours, ColorCount);
        var standard = new RgbColor[picture.StandardColours.Length];
        for (int ink = 0; ink < standard.Length; ink++)
            standard[ink] = nativeColours[InstalledPictureColoursConstants.CartridgeColourOf(picture, ink, drawRow)];
        ReadOnlySpan<RgbaPixel> pixels = picture.Pixels;
        ReadOnlySpan<SpriteImageInk> inks = picture.Inks;
        var shown = new RgbaPixel[pixels.Length];
        var indices = new byte[pixels.Length];
        var priorities = new byte[pixels.Length];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            SpriteImageInk ink = inks[pixel];
            // A transparent pixel stays transparent; every other pixel shows its ink's native colour.
            shown[pixel] = pixels[pixel].A == 0 ? pixels[pixel] : standard[ink.PaletteIndex].ToRgbaPixel() with { A = pixels[pixel].A };
            indices[pixel] = checked((byte)ink.PaletteIndex);
            priorities[pixel] = checked((byte)ink.Priority);
        }
        return SpriteImageFixtures.Copied(picture.Name, picture.Size, shown, indices,
            new SpriteImagePalette(picture.Colours, [.. standard]), priorities);
    }

    /// <summary>The sprite row <paramref name="draw"/>'s colouring ORs into its parts, or zero for a draw in no frontend colouring.</summary>
    private static int DrawRow(ImageSpriteDraw draw) => DrawRow(draw.Image.Sheet, draw.Shading);

    /// <summary>
    /// The sprite row a draw of <paramref name="picture"/> under <paramref name="shading"/> ORs into its parts: the row of
    /// the frontend colouring the shading names for a sprite picture, zero for any other.
    /// </summary>
    internal static int DrawRow(SpriteImage picture, PictureShading? shading) =>
        Ensure.NotNull(picture).Colours == SpriteImageColours.Sprite
            ? SuperMetroid.TestSupport.RepositoryInstallation.Verification.DrawnShadeRows.RowOf(shading) ?? 0
            : 0;

    /// <summary><paramref name="frame"/> cut from <paramref name="picture"/>, a native-coloured copy of its sheet.</summary>
    internal static SpriteSheetFrame Of(SpriteSheetFrame frame, SpriteImage picture)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(picture);
        return new SpriteSheetFrame(picture, frame.Corner, frame.Size, frame.Origin, frame.ColourGroups);
    }

    /// <summary>
    /// The native 256-colour image <paramref name="picture"/> shows under <paramref name="shading"/>: each ink's colour
    /// under the shading at the place the import says the ink was coloured from; the places no ink names stay black.
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
        Of(frame, Of(Ensure.NotNull(frame).Sheet, nativeColours));

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
        // A frontend draw's colouring is the row the cartridge ORs into every part (#1512); each sheet is copied once per row.
        var copies = new Dictionary<(SpriteImage Sheet, int Row), SpriteImage>();
        foreach ((SpriteImage Sheet, int Row) key in images.Select(image => (image.Image.Sheet, DrawRow(image))).Distinct())
        {
            SpriteImage copy = Of(key.Sheet, nativeColours, key.Row);
            copies.Add(key, copy);
        }
        var shown = ImmutableArray.CreateBuilder<SpriteDraw>(draws.Length);
        foreach (ImageSpriteDraw image in images)
        {
            shown.Add(new ImageSpriteDraw(image.Source, Of(image.Image, copies[(image.Image.Sheet, DrawRow(image))]), image.Origin, image.Mirror)
            {
                Shading = PictureShading.Standard,
            });
        }
        return shown.MoveToImmutable();
    }
}
