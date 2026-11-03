using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where in the cartridge's 256-colour image each ink of an installed picture was coloured from (#1512): the import's
/// knowledge, which the sheet manifest records and the loader keeps beside each picture it loads. A sprite sheet's ink is
/// a colour of the sprite half, its row times sixteen plus its colour; a gameplay background sheet's colour group was
/// installed from the background set the manifest names for it; a frontend screen's background sheet names none, and
/// its groups lie in the background half in their own order; a full-palette sheet's ink is its colour's place in the
/// image. Core never addresses a colour by these places.
/// </summary>
public static class InstalledPictureColours
{
    /// <summary>The colours of the cartridge's colour image.</summary>
    public const int CartridgeColourCount = 2 * GameplayColourPositionDefinitions.SetsColours;

    /// <summary>
    /// The sets a gameplay background sheet installed from the cartridge's background colours names, group after group:
    /// such a sheet keeps their order, so its group of each place was installed from the set of that place.
    /// </summary>
    public static ImmutableArray<InstalledBackgroundSet> CartridgeOrderSets { get; } = [.. InstalledBackgroundSetPositionDefinitions.InCartridgeOrder];

    /// <summary>The background sets each loaded picture's colour groups were installed from, as its manifest names them.</summary>
    private static readonly ConditionalWeakTable<SpriteImage, InstalledPictureBackgroundSets> Loaded = [];

    /// <summary>Keeps the background sets <paramref name="picture"/>'s manifest names for its colour groups, none for most sheets.</summary>
    /// <exception cref="InvalidDataException">
    /// The manifest names sets for a sprite or full-palette sheet, or a number of sets other than one per colour group.
    /// </exception>
    internal static void Record(SpriteImage picture, ImmutableArray<InstalledBackgroundSet> backgroundSets)
    {
        Ensure.NotNull(picture);
        if (!backgroundSets.IsEmpty &&
            (picture.Colours is not (SpriteImageColours.Background or SpriteImageColours.TwoBppBackground) ||
             backgroundSets.Length != ImageColourGroup.Count))
        {
            throw new InvalidDataException(
                $"Sprite sheet {picture.Name} names {backgroundSets.Length} background sets for a {picture.Colours} image.");
        }
        Loaded.AddOrUpdate(picture, new InstalledPictureBackgroundSets(backgroundSets));
    }

    /// <summary>The background sets <paramref name="picture"/>'s colour groups were installed from, group after group, or none.</summary>
    /// <exception cref="InvalidOperationException">The picture was not loaded from an installed sheet manifest.</exception>
    public static ImmutableArray<InstalledBackgroundSet> BackgroundSetsOf(SpriteImage picture) => SetsOf(picture).Sets;

    /// <summary>The place in the cartridge's colour image of the colour <paramref name="picture"/>'s ink <paramref name="ink"/> was coloured from.</summary>
    /// <param name="picture">An installed picture.</param>
    /// <param name="ink">One of the picture's inks: its colour group's place times sixteen plus its colour.</param>
    /// <exception cref="ArgumentOutOfRangeException">The picture has no such ink.</exception>
    /// <exception cref="InvalidOperationException">A background picture was not loaded from an installed sheet manifest.</exception>
    public static int CartridgeColourOf(SpriteImage picture, int ink)
    {
        Ensure.NotNull(picture);
        Ensure.BetweenInclusive(0, picture.StandardColours.Length - 1, ink);
        return picture.Colours switch
        {
            SpriteImageColours.Sprite => GameplayColourPositionDefinitions.SpriteSetsFirst + ink,
            SpriteImageColours.FullPalette => GameplayColourPositionDefinitions.BackgroundSetsFirst + ink,
            SpriteImageColours.Background or SpriteImageColours.TwoBppBackground => BackgroundColourOf(picture, ink),
            _ => throw new InvalidDataException($"Image {picture.Name} names an unknown colouring {picture.Colours}."),
        };
    }

    /// <summary>The place in the cartridge's colour image of a background picture's ink: in its group's installed set, or in its group's own place.</summary>
    private static int BackgroundColourOf(SpriteImage picture, int ink)
    {
        ImmutableArray<InstalledBackgroundSet> sets = SetsOf(picture).Sets;
        int group = ink / GameplayColourPositionDefinitions.SetColours;
        int set = sets.IsEmpty ? group : Array.IndexOf(InstalledBackgroundSetPositionDefinitions.InCartridgeOrder, sets[group]);
        return GameplayColourPositionDefinitions.BackgroundSetsFirst + set * GameplayColourPositionDefinitions.SetColours
            + ink % GameplayColourPositionDefinitions.SetColours;
    }

    /// <summary>What the loader kept for <paramref name="picture"/>.</summary>
    /// <exception cref="InvalidOperationException">The picture was not loaded from an installed sheet manifest.</exception>
    private static InstalledPictureBackgroundSets SetsOf(SpriteImage picture) =>
        Loaded.TryGetValue(Ensure.NotNull(picture), out InstalledPictureBackgroundSets? sets) ? sets
            : throw new InvalidOperationException($"Picture {picture.Name} was not loaded from an installed sheet manifest.");
}
