using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Checks an opening actor's drawn shade against the cartridge (#1512): the shade the installed intro sprite sheet holds
/// for a colouring shows exactly the colours of the cartridge sprite set its OAM oracle selects, so a draw that names the
/// shade shows what the cartridge's palette bits show.
/// </summary>
internal static class IntroDrawnShadeChecks
{
    /// <summary>
    /// The cartridge sprite palette row <paramref name="drawn"/> stands for, once the installed intro sprite sheet's shade
    /// <paramref name="drawn"/> is confirmed to show that row's installed colours: each of the sheet's colours it recolours
    /// shows, at every palette index its pixels were composed as, the row's colour of the same place in its group (#1544,
    /// #1633).
    /// </summary>
    internal static int InstalledRow(PictureShade drawn)
    {
        int row = DrawnShadeRows.Row(drawn);
        ImmutableArray<RgbColor> sprites = InstalledFixtures.IntroArtwork.Palette.SpriteColours;
        SpriteImage image = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.IntroSprites).Image;
        PictureRecolouring shade = image.Shade(drawn);
        AssertTrue(!shade.Substitutions.IsEmpty && shade.Substitutions.All(substitution =>
                InstalledPictureColoursConstants.ComposedIndicesOf(image, substitution.Replaced).All(index =>
                    substitution.Shown == sprites[row * ImageColourGroup.Colours + index % ImageColourGroup.Colours])),
            $"the intro sheet's {drawn.Key} shade shows the cartridge's sprite set {row}");
        return row;
    }
}
