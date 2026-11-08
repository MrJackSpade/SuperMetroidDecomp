using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The Ceres cinematics' background pictures (#1467): each SPACE COLONY caption letter, the katakana caption and the Zebes
/// planet, which the import composes once under the flight's palette. The scenes place the pictures their state shows on
/// a 32-by-32-cell plane.
/// </summary>
public static class CeresCinematicPictures
{
    /// <summary>The cells along each side of the scenes' background plane.</summary>
    internal const int PlaneCells = 32;

    /// <summary>The name of the picture of the caption's letter <paramref name="index"/>.</summary>
    public static string CaptionLetter(int index) => $"caption-{index}";

    /// <summary>The name of the katakana caption's picture.</summary>
    public const string JapaneseCaption = "caption-japanese";

    /// <summary>The name of the Zebes planet's picture.</summary>
    public const string ZebesPlanet = "zebes";

    /// <summary>
    /// The caption plane with its first <paramref name="letters"/> letters shown, and the katakana caption when
    /// <paramref name="japanese"/>, in the standard colours the import pre-coloured them in: the flight shows no colour
    /// effect on its caption, which fades out with the display's brightness (#1512).
    /// </summary>
    internal static PicturedPlane Caption(InstalledSpriteSheets sheets, int letters, bool japanese)
    {
        SpriteSheet sheet = Ensure.NotNull(sheets).Get(SpriteSheetId.CeresCinematicPictures);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        for (int letter = 0; letter < letters; letter++)
            placed.Add(new(sheet.Frame(CaptionLetter(letter)), Corner(SpaceColonyCaption.Column(letter), SpaceColonyCaptionDefinitions.Row),
                PictureShading.Standard));
        if (japanese)
            placed.Add(new(sheet.Frame(JapaneseCaption), Corner(SpaceColonyCaptionDefinitions.JapaneseColumn, SpaceColonyCaptionDefinitions.JapaneseRow),
                PictureShading.Standard));
        return new PicturedPlane(new Extent16<TilemapCells>(PlaneCells, PlaneCells), placed.ToImmutable());
    }

    /// <summary>
    /// The Zebes reveal's plane: the planet once <paramref name="revealed"/>, in its own colours (#1544), nothing before.
    /// The gunship engine's flicker is the gunship's colour effect, which the planet does not show.
    /// </summary>
    internal static PicturedPlane Zebes(InstalledSpriteSheets sheets, bool revealed) => new(
        new Extent16<TilemapCells>(PlaneCells, PlaneCells),
        revealed
            ? [new PlacedPlanePicture(Ensure.NotNull(sheets).Get(SpriteSheetId.CeresCinematicPictures).Frame(ZebesPlanet), default,
                PictureShading.Standard)]
            : []);

    /// <summary>The plane pixel of cell (<paramref name="column"/>, <paramref name="row"/>)'s upper-left corner.</summary>
    private static Position16<PlanePixels> Corner(int column, int row) => new(
        column * ScreenGeometry.BackgroundTileSizePixels, row * ScreenGeometry.BackgroundTileSizePixels);
}
