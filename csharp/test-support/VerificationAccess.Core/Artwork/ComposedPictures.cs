using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Installs pictures a check composes against the cartridge's colours as the import installs a composed sheet (#1544,
/// #1633): the picture gets its pixels, maps and shades through <see cref="InstalledSheetConversion"/>, and where each of its
/// colour keys was composed from is recorded (<see cref="InstalledPictureColours.Record"/>), with each pixel's composed
/// palette index, which only a check's oracle reads.
/// </summary>
internal static class ComposedPictures
{
    /// <summary>The composed manifest and palette indices of each picture a check composed or recorded.</summary>
    private static readonly ConditionalWeakTable<SpriteImage, ComposedPictureSource> Sources = [];

    /// <summary>The picture a sheet composed as <paramref name="composed"/> installs, with its sources recorded.</summary>
    /// <param name="name">The picture's name.</param>
    /// <param name="size">The picture's width and height.</param>
    /// <param name="pixels">The composed pixels.</param>
    /// <param name="paletteIndices">Each pixel's composed palette index.</param>
    /// <param name="composed">The composed manifest, whose colours, shades and backdrop the picture installs.</param>
    /// <param name="installed">The installed manifest, whose frames a sheet cuts.</param>
    internal static SpriteImage Install(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, SpriteSheetDocument composed, out InstalledSheetDocument installed)
    {
        InstalledSheet sheet = InstalledSheetConversion.Install(composed, size, pixels, paletteIndices);
        installed = sheet.Manifest;
        SpriteImage image = SpriteImageFixtures.Copied(name, size, sheet.Pixels, SpriteSheetFile.ColoursOf(sheet.Manifest));
        Record(image, composed, paletteIndices);
        return image;
    }

    /// <summary>
    /// The picture of <paramref name="pixels"/>, composed as <paramref name="paletteIndices"/> against
    /// <paramref name="table"/>, the colours of <paramref name="palette"/>'s half of the colour table, as the import installs
    /// it.
    /// </summary>
    internal static SpriteImage Install(string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, SpriteImageColours palette, ReadOnlySpan<RgbColor> table) =>
        Install(name, size, pixels, paletteIndices, Composed(palette, table), out _);

    /// <summary>A frameless composed manifest of <paramref name="table"/>, the colours of <paramref name="palette"/>'s half.</summary>
    internal static SpriteSheetDocument Composed(SpriteImageColours palette, ReadOnlySpan<RgbColor> table)
    {
        Ensure.LengthEqual(table, ImageColourGroup.ColourCountOf(palette));
        return new SpriteSheetDocument
        {
            Palette = palette,
            Colours = [.. table.ToArray().Select(colour => colour.ToWord())],
            Frames = [],
        };
    }

    /// <summary>
    /// Records that <paramref name="picture"/>'s pixels were composed as <paramref name="paletteIndices"/> against
    /// <paramref name="composed"/>: where each colour key came from, as the import knows it, and each pixel's index.
    /// </summary>
    /// <exception cref="ArgumentException">The indices do not hold one per pixel.</exception>
    internal static void Record(SpriteImage picture, SpriteSheetDocument composed, ReadOnlySpan<byte> paletteIndices)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(composed);
        Ensure.LengthEqual(paletteIndices, picture.Size.X * picture.Size.Y);
        InstalledPictureColours.Record(picture, composed, paletteIndices);
        Sources.AddOrUpdate(picture, new ComposedPictureSource(composed, paletteIndices.ToArray()));
    }

    /// <summary>Records for <paramref name="copy"/>, a picture of the same pixels' places, what <paramref name="picture"/> was composed from.</summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where <paramref name="picture"/> was composed from.</exception>
    internal static void RecordAs(SpriteImage copy, SpriteImage picture)
    {
        ComposedPictureSource source = SourceOf(picture);
        Record(copy, source.Composed, source.PaletteIndices);
    }

    /// <summary>What <paramref name="picture"/> was composed from.</summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where the picture was composed from.</exception>
    internal static ComposedPictureSource SourceOf(SpriteImage picture) =>
        Sources.TryGetValue(Ensure.NotNull(picture), out ComposedPictureSource? source) ? source
            : throw new InvalidOperationException($"Picture {picture.Name} was not composed or recorded by a check.");
}

/// <summary>What a picture a check composed was composed from.</summary>
/// <param name="Composed">The composed manifest: the half of the colour table and the sources of its colours.</param>
/// <param name="PaletteIndices">Each pixel's composed palette index, row by row from the top.</param>
internal sealed record ComposedPictureSource(SpriteSheetDocument Composed, byte[] PaletteIndices);
