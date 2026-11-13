using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The cartridge-ordered 256-colour image the ending's pictures show (#1512): each shade the ending's screen names, in
/// order, laid over the colours the import installs it on, from black before the first load. The screen keeps no such
/// image; verification composes it from the shading the screen publishes and the colours the import decodes for it
/// (#1544), to compare two ending states' shown colours.
/// </summary>
internal static class EndingShownColours
{
    /// <summary>The colours <paramref name="ending"/>'s pictures show now.</summary>
    /// <exception cref="KeyNotFoundException">The screen names a shade the import installs on no ending sheet.</exception>
    internal static RgbColor[] Of(EndingCreditsState ending) => Of(ending, InstalledShades(ending));

    /// <summary>
    /// The shades the import installs for <paramref name="ending"/>'s screen, by shade, from the ending's colours and the
    /// installation's palette-effect colours, which only the import reads (#1583): a check reading the shown colours of many
    /// states of one ending builds them once.
    /// </summary>
    internal static Dictionary<PictureShade, SheetShade> InstalledShades(EndingCreditsState ending)
    {
        Ensure.NotNull(ending);
        EndingPaletteSet palettes = InstalledFixtures.EndingColours;
        IPaletteFxColorSource paletteFx = InstalledFixtures.ImportMaps.RoomPaletteFx;
        return EndingScreenShades.Screen(palettes, paletteFx).ToDictionary(shade => shade.Shade);
    }

    /// <summary>The colours <paramref name="ending"/>'s pictures show now, from the shades <see cref="InstalledShades"/> built for it.</summary>
    /// <exception cref="KeyNotFoundException">The screen names a shade the import installs on no ending sheet.</exception>
    internal static RgbColor[] Of(EndingCreditsState ending, IReadOnlyDictionary<PictureShade, SheetShade> installed)
    {
        Ensure.NotNull(ending);
        Ensure.NotNull(installed);
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
