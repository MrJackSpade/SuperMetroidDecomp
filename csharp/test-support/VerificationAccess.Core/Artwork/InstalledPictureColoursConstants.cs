using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Cartridge values and helpers of <see cref="InstalledPictureColours"/> that only verification reads.</summary>
internal static class InstalledPictureColoursConstants
{
    /// <summary>The colours of the cartridge's colour image.</summary>
    public const int CartridgeColourCount = 2 * GameplayColourPositionDefinitions.SetsColours;

    /// <summary>The place in the cartridge's colour image of the colour <paramref name="picture"/>'s own colour <paramref name="colour"/> was coloured from.</summary>
    /// <param name="picture">An installed picture.</param>
    /// <param name="colour">The place of one of the picture's own colours.</param>
    /// <exception cref="ArgumentOutOfRangeException">The picture has no such colour.</exception>
    /// <exception cref="InvalidOperationException">The picture was not loaded from an installed sheet manifest.</exception>
    public static int CartridgeColourOf(SpriteImage picture, int colour) => CartridgeColourOf(picture, colour, drawRow: 0);

    /// <summary>
    /// The native colour <paramref name="colour"/> of <paramref name="picture"/> shows when a draw ORs sprite row
    /// <paramref name="drawRow"/> into its parts, as a frontend draw's colouring does (#1512).
    /// </summary>
    public static int CartridgeColourOf(SpriteImage picture, int colour, int drawRow) =>
        CartridgeColourOf(picture, InstalledPictureColours.SetsOf(Ensure.NotNull(picture)), colour, drawRow);

    /// <summary>
    /// The native colour the colour <paramref name="colour"/> of <paramref name="frame"/>'s sheet shows in a draw of the frame
    /// ORing sprite row <paramref name="drawRow"/> into its parts, where the frame's holder composed it (#1544): a picture held
    /// once on a common sheet is sourced as its first holder composed it, and another holder's colours may lie elsewhere.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet has no such colour.</exception>
    /// <exception cref="InvalidOperationException">The frame's sheet was not loaded from an installed sheet manifest.</exception>
    public static int CartridgeColourOf(SpriteSheetFrame frame, int colour, int drawRow) =>
        CartridgeColourOf(Ensure.NotNull(frame).Sheet, SourcesOf(frame), colour, drawRow);

    /// <summary>The composed palette index the colour <paramref name="colour"/> of <paramref name="frame"/>'s sheet came from, where the frame's holder composed it (#1544).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet has no such colour.</exception>
    public static int ComposedIndexOf(SpriteSheetFrame frame, int colour)
    {
        ImmutableArray<int> inks = SourcesOf(Ensure.NotNull(frame)).Inks;
        return inks[Ensure.BetweenInclusive(0, inks.Length - 1, colour)];
    }

    /// <summary>
    /// Where the colours <paramref name="frame"/> shows came from, for the holder drawing it (#1544): its sheet's record, with
    /// each colour the frame's own record names sourced as the holder composed it. The sheet's other colours, which the frame
    /// does not show, keep the sheet's indices and say nothing of the frame.
    /// </summary>
    private static InstalledPictureColourSources SourcesOf(SpriteSheetFrame frame)
    {
        InstalledPictureColourSources sheet = InstalledPictureColours.SetsOf(frame.Sheet);
        if (InstalledPictureColours.FrameSourceOf(frame) is not { } source)
            return sheet;
        ImmutableArray<int>.Builder inks = sheet.Inks.ToBuilder();
        for (int colour = 0; colour < source.SheetInks.Length; colour++)
            inks[sheet.Inks.IndexOf(source.SheetInks[colour])] = source.Inks[colour];
        return sheet with { PaletteBits = source.PaletteBits, Inks = inks.ToImmutable() };
    }

    /// <summary>The native colour <paramref name="colour"/> of <paramref name="picture"/> shows, coloured from <paramref name="sources"/>.</summary>
    private static int CartridgeColourOf(SpriteImage picture, InstalledPictureColourSources sources, int colour, int drawRow)
    {
        int ink = sources.Inks[Ensure.BetweenInclusive(0, sources.Inks.Length - 1, colour)];
        return sources.Palette switch
        {
            // A sprite ink is its part's own row and colour, drawn from that row with the sheet's palette bits ORed in.
            SpriteImageColours.Sprite => GameplayColourPositionDefinitions.SpriteSetsFirst
                + ((ink / GameplayColourPositionDefinitions.SetColours) | sources.PaletteBits | drawRow) * GameplayColourPositionDefinitions.SetColours
                + ink % GameplayColourPositionDefinitions.SetColours,
            SpriteImageColours.FullPalette => GameplayColourPositionDefinitions.BackgroundSetsFirst + ink,
            SpriteImageColours.Background or SpriteImageColours.TwoBppBackground => BackgroundColourOf(picture, ink),
            _ => throw new InvalidDataException($"Image {picture.Name} names an unknown colouring {sources.Palette}."),
        };
    }

    /// <summary>The place in the cartridge's colour image of a background picture's composed palette index: in its group's installed set, or in its group's own place.</summary>
    internal static int BackgroundColourOf(SpriteImage picture, int ink)
    {
        ImmutableArray<InstalledBackgroundSet> sets = InstalledPictureColours.SetsOf(picture).Sets;
        int group = ink / GameplayColourPositionDefinitions.SetColours;
        int set = sets.IsEmpty ? group : Array.IndexOf(InstalledBackgroundSetPositionDefinitions.InCartridgeOrder, sets[group]);
        return GameplayColourPositionDefinitions.BackgroundSetsFirst + set * GameplayColourPositionDefinitions.SetColours
            + ink % GameplayColourPositionDefinitions.SetColours;
    }

    /// <summary>The composed palette index <paramref name="picture"/>'s own colour at <paramref name="colour"/> came from.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The picture has no such colour.</exception>
    public static int ComposedIndexOf(SpriteImage picture, int colour)
    {
        ImmutableArray<int> inks = InstalledPictureColours.SetsOf(picture).Inks;
        return inks[Ensure.BetweenInclusive(0, inks.Length - 1, colour)];
    }

    /// <summary>The composed palette index of a pixel inked <paramref name="ink"/>: its colour's, or zero for a transparent pixel.</summary>
    public static int ComposedIndexOf(SpriteImage picture, SpriteImageInk ink) =>
        ink.Colour is int colour ? ComposedIndexOf(picture, colour) : 0;

    /// <summary>
    /// The place among <paramref name="picture"/>'s own colours of the colour composed as palette index
    /// <paramref name="index"/>, or null where none of its pixels shows that colour, so the picture does not have it.
    /// </summary>
    public static int? ColourComposedAs(SpriteImage picture, int index)
    {
        int place = InstalledPictureColours.SetsOf(picture).Inks.IndexOf(index);
        return place < 0 ? null : place;
    }

    /// <summary>
    /// <paramref name="colours"/>, one colour for each of <paramref name="picture"/>'s own colours, laid out by the composed
    /// palette index each came from in a list as long as the half of the colour table the picture was composed against;
    /// an index the picture has no colour of stays black.
    /// </summary>
    public static RgbColor[] ByComposedIndex(SpriteImage picture, ReadOnlySpan<RgbColor> colours)
    {
        InstalledPictureColourSources sources = InstalledPictureColours.SetsOf(picture);
        Ensure.LengthEqual(colours, sources.Inks.Length);
        var laid = new RgbColor[ImageColourGroup.ColourCountOf(sources.Palette)];
        for (int colour = 0; colour < colours.Length; colour++)
            laid[sources.Inks[colour]] = colours[colour];
        return laid;
    }

    /// <summary><paramref name="picture"/>'s standard colours laid out by composed palette index, as <see cref="ByComposedIndex"/> lays them out.</summary>
    public static RgbColor[] ComposedColours(SpriteImage picture) => ByComposedIndex(picture, Ensure.NotNull(picture).StandardColours);

    /// <summary>
    /// Whether each of <paramref name="picture"/>'s colours shows, in <paramref name="shown"/>, the colour of
    /// <paramref name="native"/>, a whole native colour image, at the place it was coloured from.
    /// </summary>
    public static bool ShowsNativeColours(SpriteImage picture, ReadOnlySpan<RgbColor> shown, ReadOnlySpan<RgbColor> native)
    {
        Ensure.NotNull(picture);
        Ensure.LengthEqual(shown, picture.StandardColours.Length);
        Ensure.LengthEqual(native, CartridgeColourCount);
        for (int colour = 0; colour < shown.Length; colour++)
        {
            if (shown[colour] != native[CartridgeColourOf(picture, colour)])
                return false;
        }
        return true;
    }

    /// <summary>Records for <paramref name="copy"/> the source <paramref name="picture"/>'s manifest named.</summary>
    public static void RecordAs(SpriteImage copy, SpriteImage picture)
    {
        InstalledPictureColourSources sources = InstalledPictureColours.SetsOf(picture);
        InstalledPictureColours.Record(copy, new InstalledSheetSource
        {
            Palette = sources.Palette,
            PaletteBits = sources.PaletteBits,
            BackgroundSets = sources.Sets.IsEmpty ? null : [.. sources.Sets],
            Inks = [.. sources.Inks],
        });
    }
}
