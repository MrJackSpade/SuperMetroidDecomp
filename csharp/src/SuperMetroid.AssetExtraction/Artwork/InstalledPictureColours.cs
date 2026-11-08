using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where in the cartridge's 256-colour image each of an installed picture's own colours was coloured from (#1512, #1544):
/// the import's knowledge, which the sheet manifest records (<see cref="InstalledSheetSource"/>) and the loader keeps beside
/// each picture it loads. Each colour came from a composed palette index of the half of the colour table the sheet was
/// composed against. A sprite sheet's index is its part's own row times sixteen plus its colour, coloured from the sprite
/// half's row of that row with the sheet's palette bits ORed in; a gameplay background sheet's colour group was installed
/// from the background set the manifest names for it; a frontend screen's background sheet names none, and its groups lie
/// in the background half in their own order; a full-palette sheet's index is its colour's place in the image. Core never
/// addresses a colour by these places.
/// </summary>
public static class InstalledPictureColours
{

    /// <summary>
    /// The sets a gameplay background sheet installed from the cartridge's background colours names, group after group:
    /// such a sheet keeps their order, so its group of each place was installed from the set of that place.
    /// </summary>
    public static ImmutableArray<InstalledBackgroundSet> CartridgeOrderSets { get; } = [.. InstalledBackgroundSetPositionDefinitions.InCartridgeOrder];

    /// <summary>Where each loaded picture's colours came from, as its manifest names it.</summary>
    private static readonly ConditionalWeakTable<SpriteImage, InstalledPictureColourSources> Loaded = [];

    /// <summary>The sprite rows a sprite sheet's palette bits select among.</summary>
    private const int SpriteRows = 8;

    /// <summary>Keeps where <paramref name="picture"/>'s manifest says each of its colours came from.</summary>
    /// <exception cref="InvalidDataException">
    /// The manifest names sets for a sprite sheet, which has no background groups, or a number of sets other than one per
    /// background colour group, or palette bits for a picture other than a sprite sheet or past the sprite rows, or a
    /// composed palette index for each colour other than one per colour, past the half, or repeated.
    /// </exception>
    internal static void Record(SpriteImage picture, InstalledSheetSource source)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(source);
        ImmutableArray<InstalledBackgroundSet> backgroundSets = [.. source.BackgroundSets ?? []];
        if (!backgroundSets.IsEmpty &&
            (source.Palette == SpriteImageColours.Sprite ||
             backgroundSets.Length != ImageColourGroup.Count))
        {
            throw new InvalidDataException(
                $"Sprite sheet {picture.Name} names {backgroundSets.Length} background sets for a {source.Palette} image.");
        }
        if (source.PaletteBits != 0 && (source.Palette != SpriteImageColours.Sprite || source.PaletteBits is < 0 or >= SpriteRows))
            throw new InvalidDataException($"Sheet {picture.Name} names palette bits {source.PaletteBits} for a {source.Palette} image.");
        ImmutableArray<int> inks = [.. source.Inks ?? []];
        int half = ImageColourGroup.ColourCountOf(source.Palette);
        if (inks.Length != picture.StandardColours.Length || inks.Any(ink => ink < 0 || ink >= half) || inks.Distinct().Count() != inks.Length)
            throw new InvalidDataException($"Sheet {picture.Name} does not name one composed palette index for each of its {picture.StandardColours.Length} colours.");
        Loaded.AddOrUpdate(picture, new InstalledPictureColourSources(source.Palette, backgroundSets, source.PaletteBits, inks));
    }

    /// <summary>The palette bits <paramref name="picture"/>'s draws OR into each ink's own row, zero for most pictures.</summary>
    /// <exception cref="InvalidOperationException">The picture was not loaded from an installed sheet manifest.</exception>
    public static int PaletteBitsOf(SpriteImage picture) => SetsOf(picture).PaletteBits;

    /// <summary>The background sets <paramref name="picture"/>'s colour groups were installed from, group after group, or none.</summary>
    /// <exception cref="InvalidOperationException">The picture was not loaded from an installed sheet manifest.</exception>
    public static ImmutableArray<InstalledBackgroundSet> BackgroundSetsOf(SpriteImage picture) => SetsOf(picture).Sets;

    /// <summary>What the loader kept for <paramref name="picture"/>.</summary>
    /// <exception cref="InvalidOperationException">The picture was not loaded from an installed sheet manifest.</exception>
    internal static InstalledPictureColourSources SetsOf(SpriteImage picture) =>
        Loaded.TryGetValue(Ensure.NotNull(picture), out InstalledPictureColourSources? sets) ? sets
            : throw new InvalidOperationException($"Picture {picture.Name} was not loaded from an installed sheet manifest.");
}
