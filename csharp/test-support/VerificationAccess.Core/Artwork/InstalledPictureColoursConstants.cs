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
    internal static int BackgroundColourOf(SpriteImage picture, int ink)
    {
        ImmutableArray<InstalledBackgroundSet> sets = InstalledPictureColours.SetsOf(picture).Sets;
        int group = ink / GameplayColourPositionDefinitions.SetColours;
        int set = sets.IsEmpty ? group : Array.IndexOf(InstalledBackgroundSetPositionDefinitions.InCartridgeOrder, sets[group]);
        return GameplayColourPositionDefinitions.BackgroundSetsFirst + set * GameplayColourPositionDefinitions.SetColours
            + ink % GameplayColourPositionDefinitions.SetColours;
    }
}
