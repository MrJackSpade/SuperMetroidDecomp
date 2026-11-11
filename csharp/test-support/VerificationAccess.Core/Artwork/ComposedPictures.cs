using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Pictures a check composes against the cartridge's colours, installed as the import installs a composed sheet (#1544,
/// #1633): the picture gets its pixels, maps and shades through <see cref="InstalledSheetConversion"/>, and where each of its
/// colour keys and each of its pixels was composed from is recorded here, the import's knowledge, which no installed
/// picture holds and only a check's native-colour oracle reads. An installed picture is never asked for its palette
/// indices: a check that needs them composes the picture's sheet in-process (<see cref="RecordComposition"/>,
/// <see cref="SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledSheetCompositions"/>).
/// </summary>
internal static class ComposedPictures
{
    /// <summary>Where each picture a check composed or recorded was composed from.</summary>
    private static readonly ConditionalWeakTable<SpriteImage, ComposedPictureSource> Sources = [];

    /// <summary>The sprite rows a sprite sheet's palette bits select among.</summary>
    private const int SpriteRows = 8;

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
    /// Records that <paramref name="installed"/>, a picture an installation holds, is the installed form of
    /// <paramref name="composition"/>, its sheet composed again in-process: the composition is installed through
    /// <see cref="InstalledSheetConversion"/> and its pixels compared with the installed picture's by colour, so the
    /// composition's palette indices describe the installed picture's pixels.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed picture's pixels differ from its composition's, as an edited installation's do.</exception>
    internal static void RecordComposition(SpriteImage installed, ComposedSpriteSheet composition)
    {
        Ensure.NotNull(installed);
        Ensure.NotNull(composition);
        if (composition.Size != installed.Size)
            throw new InvalidDataException($"Installed picture {installed} is not the size of its composition, {composition.Size.X}x{composition.Size.Y}.");
        InstalledSheet converted = InstalledSheetConversion.Install(composition.Manifest, composition.Size, composition.Pixels, composition.PaletteIndices);
        if (!installed.Pixels.SequenceEqual(converted.Pixels.Pixels)
            || !installed.RecolourRegions.SequenceEqual(converted.Pixels.RecolourRegions ?? []))
            throw new InvalidDataException($"Installed picture {installed} does not show the pixels its sheet composes; a check comparing it with the cartridge needs an unedited installation.");
        Record(installed, composition.Manifest, composition.PaletteIndices);
    }

    /// <summary>
    /// Records that <paramref name="picture"/>'s pixels were composed as <paramref name="paletteIndices"/> against
    /// <paramref name="composed"/>: where each of its colour keys came from, as the import knows it, and each pixel's index.
    /// </summary>
    /// <exception cref="ArgumentException">The indices do not hold one per pixel.</exception>
    /// <exception cref="InvalidDataException">
    /// The manifest names background sets for a sprite sheet or a number of sets other than one per background colour group,
    /// palette bits for a picture other than a sprite sheet or past the sprite rows, or the picture's backdrop without
    /// naming a backdrop index.
    /// </exception>
    internal static void Record(SpriteImage picture, SpriteSheetDocument composed, ReadOnlySpan<byte> paletteIndices)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(composed);
        Ensure.LengthEqual(paletteIndices, picture.Size.X * picture.Size.Y);
        ImmutableArray<InstalledBackgroundSet> sets = [.. composed.BackgroundSets ?? []];
        if (!sets.IsEmpty && (composed.Palette == SpriteImageColours.Sprite || sets.Length != ImageColourGroup.Count))
            throw new InvalidDataException($"Sheet {picture.Name} names {sets.Length} background sets for a {composed.Palette} image.");
        if (composed.PaletteBits != 0 && (composed.Palette != SpriteImageColours.Sprite || composed.PaletteBits is < 0 or >= SpriteRows))
            throw new InvalidDataException($"Sheet {picture.Name} names palette bits {composed.PaletteBits} for a {composed.Palette} image.");
        var indices = new Dictionary<ColourKey, SortedSet<int>>();
        for (int pixel = 0; pixel < paletteIndices.Length; pixel++)
            Add(indices, picture.KeyAt(pixel), paletteIndices[pixel]);
        if (picture.Backdrop is { } backdrop)
            Add(indices, backdrop, composed.Backdrop ?? throw new InvalidDataException($"Sheet {picture.Name} shows a backdrop its composition names no index of."));
        Sources.AddOrUpdate(picture, new ComposedPictureSource(composed, paletteIndices.ToArray(),
            indices.ToImmutableDictionary(entry => entry.Key, entry => entry.Value.ToImmutableArray())));
    }

    /// <summary>Adds <paramref name="index"/> to the palette indices <paramref name="key"/> was composed as.</summary>
    private static void Add(Dictionary<ColourKey, SortedSet<int>> indices, ColourKey key, int index)
    {
        if (!indices.TryGetValue(key, out SortedSet<int>? composed))
            indices.Add(key, composed = []);
        composed.Add(index);
    }

    /// <summary>Records for <paramref name="copy"/>, a picture of the same pixels' places, what <paramref name="picture"/> was composed from.</summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where <paramref name="picture"/> was composed from.</exception>
    internal static void RecordAs(SpriteImage copy, SpriteImage picture)
    {
        ComposedPictureSource source = SourceOf(picture);
        Record(copy, source.Composed, source.PaletteIndices);
    }

    /// <summary>
    /// What <paramref name="picture"/> was composed from: as a check recorded it, or, for a picture an installation holds,
    /// as its sheet composes in-process (<see cref="SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledSheetCompositions"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">No check recorded the picture and no installed family composes a sheet of its name.</exception>
    internal static ComposedPictureSource SourceOf(SpriteImage picture)
    {
        Ensure.NotNull(picture);
        lock (Sources)
        {
            if (!Sources.TryGetValue(picture, out ComposedPictureSource? source))
            {
                RecordComposition(picture, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledSheetCompositions.Compose(picture.Name));
                source = Sources.TryGetValue(picture, out ComposedPictureSource? composed) ? composed
                    : throw new InvalidOperationException($"Picture {picture.Name} was not recorded.");
            }
            return source;
        }
    }
}

/// <summary>What a picture a check composed was composed from.</summary>
/// <param name="Composed">The composed manifest: the half of the colour table, the background sets and the palette bits.</param>
/// <param name="PaletteIndices">Each pixel's composed palette index, row by row from the top.</param>
/// <param name="ComposedIndices">
/// Each colour key the picture holds (its pixels' keys and its backdrop's), with the palette indices, ascending, its pixels
/// were composed as.
/// </param>
internal sealed record ComposedPictureSource(SpriteSheetDocument Composed, byte[] PaletteIndices,
    ImmutableDictionary<ColourKey, ImmutableArray<int>> ComposedIndices)
{
    /// <summary>Which half of the colour table the picture was composed against.</summary>
    internal SpriteImageColours Palette => Composed.Palette;

    /// <summary>The palette bits the picture's draws OR into each part's own row, zero for most pictures.</summary>
    internal int PaletteBits => Composed.PaletteBits;

    /// <summary>The background sets the picture's colour groups were installed from, group after group, or none.</summary>
    internal ImmutableArray<InstalledBackgroundSet> Sets => [.. Composed.BackgroundSets ?? []];
}
