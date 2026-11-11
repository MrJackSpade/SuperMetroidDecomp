using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Cartridge values and helpers of <see cref="ComposedPictures"/> that only verification reads (#1633): where in the
/// cartridge's colour image each colour key of a picture a check composed was coloured from. A key may have been composed
/// from several palette indices, each of which the port shows alike; a check compares every one.
/// </summary>
internal static class InstalledPictureColoursConstants
{
    /// <summary>The colours of the cartridge's colour image.</summary>
    public const int CartridgeColourCount = 2 * GameplayColourPositionDefinitions.SetsColours;

    /// <summary>The composed palette indices the pixels of <paramref name="key"/> of <paramref name="picture"/> were composed as, ascending.</summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where the picture was composed from.</exception>
    /// <exception cref="ArgumentException">The picture holds no such key.</exception>
    public static ImmutableArray<int> ComposedIndicesOf(SpriteImage picture, ColourKey key) =>
        ComposedPictures.SourceOf(Ensure.NotNull(picture)).ComposedIndices.TryGetValue(key, out ImmutableArray<int> indices) ? indices
            : throw new ArgumentException($"Picture {picture.Name} holds no colour key {key}.", nameof(key));

    /// <summary>
    /// The composed palette indices of <paramref name="key"/> of <paramref name="picture"/> that show a colour, ascending:
    /// every one but colour zero of an image other than a HUD or room effect picture, which the cartridge draws transparent
    /// and colours from nowhere.
    /// </summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where the picture was composed from.</exception>
    public static ImmutableArray<int> ColouredIndicesOf(SpriteImage picture, ColourKey key)
    {
        SpriteImageColours palette = ComposedPictures.SourceOf(Ensure.NotNull(picture)).Palette;
        return [.. ComposedIndicesOf(picture, key).Where(index => palette == SpriteImageColours.TwoBppBackground || index != 0)];
    }

    /// <summary>
    /// The places in the cartridge's colour image of the colours <paramref name="key"/> of <paramref name="picture"/> was
    /// coloured from, when a draw ORs sprite row <paramref name="drawRow"/> into its parts, as a frontend draw's colouring
    /// does (#1512).
    /// </summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where the picture was composed from.</exception>
    public static ImmutableArray<int> CartridgeColoursOf(SpriteImage picture, ColourKey key, int drawRow = 0) =>
        [.. ColouredIndicesOf(picture, key).Select(index => CartridgeColourOfIndex(picture, index, drawRow))];

    /// <summary>
    /// The place in the cartridge's colour image a pixel of <paramref name="picture"/> composed as palette index
    /// <paramref name="index"/> is coloured from, when a draw ORs sprite row <paramref name="drawRow"/> into its parts.
    /// </summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where the picture was composed from.</exception>
    public static int CartridgeColourOfIndex(SpriteImage picture, int index, int drawRow = 0)
    {
        ComposedPictureSource sources = ComposedPictures.SourceOf(Ensure.NotNull(picture));
        Ensure.BetweenInclusive(0, ImageColourGroup.ColourCountOf(sources.Palette) - 1, index);
        return sources.Palette switch
        {
            // A sprite index is its part's own row and colour, drawn from that row with the sheet's palette bits ORed in.
            SpriteImageColours.Sprite => GameplayColourPositionDefinitions.SpriteSetsFirst
                + ((index / GameplayColourPositionDefinitions.SetColours) | sources.PaletteBits | drawRow) * GameplayColourPositionDefinitions.SetColours
                + index % GameplayColourPositionDefinitions.SetColours,
            SpriteImageColours.FullPalette => GameplayColourPositionDefinitions.BackgroundSetsFirst + index,
            SpriteImageColours.Background or SpriteImageColours.TwoBppBackground => BackgroundColourOf(sources, index),
            _ => throw new InvalidDataException($"Image {picture.Name} names an unknown colouring {sources.Palette}."),
        };
    }

    /// <summary>The place in the cartridge's colour image of a background picture's composed palette index: in its group's installed set, or in its group's own place.</summary>
    private static int BackgroundColourOf(ComposedPictureSource sources, int index)
    {
        ImmutableArray<InstalledBackgroundSet> sets = sources.Sets;
        int group = index / GameplayColourPositionDefinitions.SetColours;
        int set = sets.IsEmpty ? group : Array.IndexOf(InstalledBackgroundSetPositionDefinitions.InCartridgeOrder, sets[group]);
        return GameplayColourPositionDefinitions.BackgroundSetsFirst + set * GameplayColourPositionDefinitions.SetColours
            + index % GameplayColourPositionDefinitions.SetColours;
    }

    /// <summary>The composed palette index pixel <paramref name="pixel"/> of <paramref name="picture"/> was composed as.</summary>
    /// <exception cref="InvalidOperationException">The picture was not composed or recorded by a check.</exception>
    public static int ComposedIndexAt(SpriteImage picture, int pixel)
    {
        byte[] indices = ComposedPictures.SourceOf(picture).PaletteIndices;
        return indices[Ensure.BetweenInclusive(0, indices.Length - 1, pixel)];
    }

    /// <summary>
    /// The key of <paramref name="picture"/> whose pixels were composed as palette index <paramref name="index"/>, or null
    /// where none of its pixels was or the index shows no colour (<see cref="ColouredIndicesOf"/>), so the picture does not
    /// show that colour.
    /// </summary>
    /// <exception cref="InvalidOperationException">Nothing recorded where the picture was composed from.</exception>
    public static ColourKey? KeyComposedAs(SpriteImage picture, int index)
    {
        foreach (ColourKey key in ComposedPictures.SourceOf(Ensure.NotNull(picture)).ComposedIndices.Keys)
        {
            if (ColouredIndicesOf(picture, key).Contains(index))
                return key;
        }
        return null;
    }

    /// <summary>
    /// <paramref name="shown"/>, one colour for each of <paramref name="picture"/>'s colour keys
    /// (<see cref="SpriteImage.ColourKeys"/>), laid out by each palette index the key was composed as, in a list as long as
    /// the half of the colour table the picture was composed against; an index the picture has no pixel of, or that shows no
    /// colour (<see cref="ColouredIndicesOf"/>), stays black.
    /// </summary>
    public static RgbColor[] ByComposedIndex(SpriteImage picture, ReadOnlySpan<RgbColor> shown)
    {
        Ensure.NotNull(picture);
        ImmutableArray<ColourKey> keys = picture.ColourKeys;
        Ensure.LengthEqual(shown, keys.Length);
        var laid = new RgbColor[ImageColourGroup.ColourCountOf(ComposedPictures.SourceOf(picture).Palette)];
        for (int key = 0; key < keys.Length; key++)
        {
            foreach (int index in ColouredIndicesOf(picture, keys[key]))
                laid[index] = shown[key];
        }
        return laid;
    }

    /// <summary><paramref name="picture"/>'s own colours laid out by composed palette index, as <see cref="ByComposedIndex"/> lays them out.</summary>
    public static RgbColor[] ComposedColours(SpriteImage picture) =>
        ByComposedIndex(picture, [.. Ensure.NotNull(picture).ColourKeys.Select(key => key.Colour)]);

    /// <summary>
    /// Whether each of <paramref name="picture"/>'s colour keys shows, in <paramref name="shown"/> (one colour per key), the
    /// colour of <paramref name="native"/>, a whole native colour image, at every place it was coloured from
    /// (<see cref="CartridgeColoursOf"/>).
    /// </summary>
    public static bool ShowsNativeColours(SpriteImage picture, ReadOnlySpan<RgbColor> shown, ReadOnlySpan<RgbColor> native)
    {
        Ensure.NotNull(picture);
        ImmutableArray<ColourKey> keys = picture.ColourKeys;
        Ensure.LengthEqual(shown, keys.Length);
        Ensure.LengthEqual(native, CartridgeColourCount);
        for (int key = 0; key < keys.Length; key++)
        {
            foreach (int place in CartridgeColoursOf(picture, keys[key]))
            {
                if (shown[key] != native[place])
                    return false;
            }
        }
        return true;
    }
}
