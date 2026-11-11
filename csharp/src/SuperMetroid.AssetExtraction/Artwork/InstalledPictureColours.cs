using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where in the cartridge's 256-colour image the colours of a picture the import composed were coloured from (#1512, #1544,
/// #1633): the import's knowledge, recorded beside a picture composed against the cartridge so its composition can be
/// confirmed against the cartridge. Each colour came from a composed palette index of the half of the colour table the
/// sheet was composed against. A sprite sheet's index is its part's own row times sixteen plus its colour, coloured from
/// the sprite half's row of that row with the sheet's palette bits ORed in; a gameplay background sheet's colour group was
/// installed from the background set the composed manifest names for it; a frontend screen's background sheet names none,
/// and its groups lie in the background half in their own order; a full-palette sheet's index is its colour's place in the
/// image. Nothing of it is installed: an installed picture is only its colours, and Core never addresses a colour by these
/// indices.
/// </summary>
public static class InstalledPictureColours
{
    /// <summary>
    /// The sets a gameplay background sheet installed from the cartridge's background colours names, group after group:
    /// such a sheet keeps their order, so its group of each place was installed from the set of that place.
    /// </summary>
    public static ImmutableArray<InstalledBackgroundSet> CartridgeOrderSets { get; } = [.. InstalledBackgroundSetPositionDefinitions.InCartridgeOrder];

    /// <summary>Where each recorded picture's colours came from.</summary>
    private static readonly ConditionalWeakTable<SpriteImage, InstalledPictureColourSources> Recorded = [];

    /// <summary>The sprite rows a sprite sheet's palette bits select among.</summary>
    private const int SpriteRows = 8;

    /// <summary>
    /// Keeps where the colours of <paramref name="picture"/>, installed from a sheet composed as <paramref name="composed"/>
    /// whose pixels were composed as <paramref name="paletteIndices"/>, came from: for each colour key of its visible pixels,
    /// the composed indices of its pixels of that key, and the backdrop's index under the backdrop's key. A clear pixel is
    /// never keyed.
    /// </summary>
    /// <exception cref="ArgumentException">The indices are not one per pixel of the picture.</exception>
    /// <exception cref="InvalidDataException">
    /// The composed manifest names sets for a sprite sheet, which has no background groups, or a number of sets other than
    /// one per background colour group, or palette bits for a picture other than a sprite sheet or past the sprite rows.
    /// </exception>
    internal static void Record(SpriteImage picture, SpriteSheetDocument composed, ReadOnlySpan<byte> paletteIndices)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(composed);
        ImmutableArray<InstalledBackgroundSet> backgroundSets = [.. composed.BackgroundSets ?? []];
        if (!backgroundSets.IsEmpty &&
            (composed.Palette == SpriteImageColours.Sprite ||
             backgroundSets.Length != ImageColourGroup.Count))
        {
            throw new InvalidDataException(
                $"Sprite sheet {picture.Name} names {backgroundSets.Length} background sets for a {composed.Palette} image.");
        }
        if (composed.PaletteBits != 0 && (composed.Palette != SpriteImageColours.Sprite || composed.PaletteBits is < 0 or >= SpriteRows))
            throw new InvalidDataException($"Sheet {picture.Name} names palette bits {composed.PaletteBits} for a {composed.Palette} image.");
        Ensure.LengthEqual(paletteIndices, picture.Size.X * picture.Size.Y);
        var indices = new Dictionary<ColourKey, SortedSet<int>>();
        for (int pixel = 0; pixel < paletteIndices.Length; pixel++)
        {
            if (InstalledSheetConversion.KindOf(composed.Palette, paletteIndices[pixel]) is ComposedPixelKind.Opaque or ComposedPixelKind.Blending)
                IndicesOf(indices, picture.KeyAt(pixel)).Add(paletteIndices[pixel]);
        }
        if (picture.Backdrop is { } backdrop && composed.Backdrop is { } index)
            IndicesOf(indices, backdrop).Add(index);
        Recorded.AddOrUpdate(picture, new InstalledPictureColourSources(composed.Palette, backgroundSets, composed.PaletteBits,
            indices.ToImmutableDictionary(entry => entry.Key, entry => entry.Value.ToImmutableArray())));
    }

    /// <summary>The indices found so far of <paramref name="key"/>, started empty where it is new.</summary>
    private static SortedSet<int> IndicesOf(Dictionary<ColourKey, SortedSet<int>> indices, ColourKey key)
    {
        if (!indices.TryGetValue(key, out SortedSet<int>? found))
            indices.Add(key, found = []);
        return found;
    }

    /// <summary>What was recorded for <paramref name="picture"/>.</summary>
    /// <exception cref="InvalidOperationException">The picture's colours were not recorded: it was not composed against the cartridge here.</exception>
    internal static InstalledPictureColourSources SetsOf(SpriteImage picture) =>
        Recorded.TryGetValue(Ensure.NotNull(picture), out InstalledPictureColourSources? sets) ? sets
            : throw new InvalidOperationException($"Picture {picture.Name} has no record of where its colours were composed from.");
}
