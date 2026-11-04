using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1512): the cartridge sprite palette row a frontend draw's drawn shade stands for, as the import
/// places each colouring's colours, so a draw that names its shading can still be compared with cartridge OAM oracles,
/// whose parts select their row by palette bits.
/// </summary>
internal static class DrawnShadeRows
{
    /// <summary>The row each opening, Ceres, title and map sprite colouring's drawn shade stands for.</summary>
    private static readonly FrozenDictionary<PictureShade, int> Rows = Enum.GetValues<IntroSpriteColouring>()
        .Select(colouring => (Shade: IntroShades.Drawn(colouring), Row: IntroSheetInks.SpriteSet(colouring)))
        .Concat(Enum.GetValues<CeresSpriteColouring>()
            .Select(colouring => (Shade: CeresShades.Drawn(colouring), Row: CeresSheetInks.SpriteSet(colouring))))
        .Concat(Enum.GetValues<TitleSpriteColouring>()
            .Select(colouring => (Shade: TitleShades.Drawn(colouring), Row: TitleSheetInks.SpriteSet(colouring))))
        .Concat(Enum.GetValues<MapSpriteColouring>().SelectMany(colouring => Enum.GetValues<MapSpriteScreen>()
            .Select(screen => (Shade: MapSpriteShades.Drawn(screen, colouring), Row: MapSpriteColouringSets.SpriteSet(colouring)))))
        .ToFrozenDictionary(entry => entry.Shade, entry => entry.Row);

    /// <summary>The row the drawn shade <paramref name="drawn"/> stands for.</summary>
    /// <exception cref="InvalidOperationException">The shade is no opening, Ceres or title colouring's drawn shade.</exception>
    internal static int Row(PictureShade drawn) => Rows.TryGetValue(drawn, out int row) ? row
        : throw new InvalidOperationException($"{drawn.Key} is no opening, Ceres or title colouring's drawn shade.");

    /// <summary>
    /// The row the first drawn shade of <paramref name="shading"/> stands for, or null when the draw names no shading or
    /// none of its shades is an opening, Ceres or title colouring's drawn shade.
    /// </summary>
    internal static int? RowOf(PictureShading? shading)
    {
        if (shading is null)
            return null;
        foreach (PictureShade shade in shading.Shades)
        {
            if (Rows.TryGetValue(shade, out int row))
                return row;
        }
        return null;
    }
}
