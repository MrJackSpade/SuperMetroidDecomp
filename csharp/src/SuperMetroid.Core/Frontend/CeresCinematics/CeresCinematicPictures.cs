using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The Ceres cinematics' background pictures (#1467): each SPACE COLONY caption letter, the katakana caption and the Zebes
/// planet, which the import composes once under the flight's palette. The scenes place the pictures their state shows on
/// a plane, each at its cell.
/// </summary>
public static class CeresCinematicPictures
{
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
            placed.Add(new(sheet.Frame(CaptionLetter(letter)), new Offset16<PlanePixels>(SpaceColonyCaption.Left(letter), SpaceColonyCaptionDefinitions.Top),
                PictureShading.Standard));
        if (japanese)
            placed.Add(new(sheet.Frame(JapaneseCaption), new Offset16<PlanePixels>(SpaceColonyCaptionDefinitions.JapaneseLeft, SpaceColonyCaptionDefinitions.JapaneseTop),
                PictureShading.Standard));
        return new PicturedPlane(placed.ToImmutable());
    }

    /// <summary>
    /// The Zebes reveal's plane: the planet once <paramref name="revealed"/>, in its own colours (#1544), nothing before.
    /// The gunship engine's flicker is the gunship's colour effect, which the planet does not show.
    /// </summary>
    internal static PicturedPlane Zebes(InstalledSpriteSheets sheets, bool revealed) => new(
        revealed
            ? [new PlacedPlanePicture(Ensure.NotNull(sheets).Get(SpriteSheetId.CeresCinematicPictures).Frame(ZebesPlanet), default,
                PictureShading.Standard)]
            : []);
}
