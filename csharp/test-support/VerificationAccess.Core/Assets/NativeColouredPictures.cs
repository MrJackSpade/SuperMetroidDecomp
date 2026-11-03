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
    internal const int ColorCount = InstalledPictureColours.CartridgeColourCount;

    /// <summary>
    /// A copy of <paramref name="picture"/> whose standard colours are the colours its inks show in
    /// <paramref name="nativeColours"/>; its name, pixels, inks, priorities and part groups are kept, so fixtures that pair
    /// an installed frame with its composition by name find the copy's too, and it has no shades.
    /// </summary>
    /// <exception cref="ArgumentException">The image does not hold 256 colours.</exception>
    [WholeImageFixture("a picture coloured from a 256-colour native colour image")]
    internal static SpriteImage Of(SpriteImage picture, ReadOnlySpan<RgbColor> nativeColours)
    {
        Ensure.NotNull(picture);
        Ensure.LengthEqual(nativeColours, ColorCount);
        var standard = new RgbColor[picture.StandardColours.Length];
        for (int ink = 0; ink < standard.Length; ink++)
            standard[ink] = nativeColours[InstalledPictureColours.CartridgeColourOf(picture, ink)];
        ReadOnlySpan<RgbaPixel> pixels = picture.Pixels;
        ReadOnlySpan<SpriteImageInk> inks = picture.Inks;
        var shown = new RgbaPixel[pixels.Length];
        var indices = new byte[pixels.Length];
        var priorities = new byte[pixels.Length];
        var partGroups = new byte[pixels.Length];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            SpriteImageInk ink = inks[pixel];
            // A transparent pixel stays transparent; every other pixel shows its ink's native colour.
            shown[pixel] = pixels[pixel].A == 0 ? pixels[pixel] : standard[ink.PaletteIndex].ToRgbaPixel() with { A = pixels[pixel].A };
            indices[pixel] = checked((byte)ink.PaletteIndex);
            priorities[pixel] = checked((byte)ink.Priority);
            partGroups[pixel] = checked((byte)ink.PartGroup.Code);
        }
        return new SpriteImage(picture.Name, picture.Size, shown, indices,
            new SpriteImagePalette(picture.Colours, [.. standard]), priorities, partGroups);
    }

    /// <summary><paramref name="frame"/> and its parts cut from <paramref name="picture"/>, a native-coloured copy of its sheet.</summary>
    internal static SpriteSheetFrame Of(SpriteSheetFrame frame, SpriteImage picture)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(picture);
        return new SpriteSheetFrame(picture, frame.Corner, frame.Size, frame.Origin, frame.ColourGroups,
            [.. frame.Parts.Select(part => part with { Image = Of(part.Image, picture) })]);
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
            image[InstalledPictureColours.CartridgeColourOf(picture, ink)] = inks[ink];
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
        var copies = new Dictionary<SpriteImage, SpriteImage>();
        foreach (SpriteImage sheet in images.Select(image => image.Image.Sheet).Distinct())
        {
            // Each sheet is copied once, however many draws show it.
            SpriteImage copy = Of(sheet, nativeColours);
            copies.Add(sheet, copy);
        }
        var shown = ImmutableArray.CreateBuilder<SpriteDraw>(draws.Length);
        foreach (ImageSpriteDraw image in images)
        {
            shown.Add(new ImageSpriteDraw(image.Source, Of(image.Image, copies[image.Image.Sheet]), image.Origin, image.Mirror)
            {
                Shading = PictureShading.Standard,
            });
        }
        return shown.MoveToImmutable();
    }
}
