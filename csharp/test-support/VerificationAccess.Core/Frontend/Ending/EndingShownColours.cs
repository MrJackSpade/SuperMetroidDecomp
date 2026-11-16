using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The cartridge-ordered 256-colour image the ending's pictures show (#1512): each shade the ending's screen names, in
/// order, laid over the colours the import installs it on, from black before the first load. The screen keeps no such
/// image; verification composes it from the shading the screen publishes and the colours the import decodes for it
/// (#1544), to compare two ending states' shown colours.
/// </summary>
internal static class EndingShownColours
{
    /// <summary>
    /// The colours <paramref name="ending"/>'s pictures show now, through the shades the import installs for the ending's
    /// screen (<see cref="InstalledFixtures.EndingScreenShadesByShade"/>), which every ending state shares and the process
    /// builds once.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The screen names a shade the import installs on no ending sheet.</exception>
    internal static RgbColor[] Of(EndingCreditsState ending)
    {
        Ensure.NotNull(ending);
        IReadOnlyDictionary<PictureShade, SheetShade> installed = InstalledFixtures.EndingScreenShadesByShade;
        var colours = new RgbColor[EndingScreenInks.ScreenColourCount];
        foreach (PictureShade shown in ending.screen.Colours.Shading.Shades)
        {
            SheetShade shade = installed[shown];
            for (int index = 0; index < shade.Inks.Length; index++)
                colours[shade.Inks[index]] = shade.Colours[index];
        }
        return colours;
    }
}
