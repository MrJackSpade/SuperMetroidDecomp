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

    /// <summary>The name of every picture the sheet holds.</summary>
    public static IEnumerable<string> FrameNames =>
        [.. Enumerable.Range(0, SpaceColonyCaptionDefinitions.LetterCount).Select(CaptionLetter), JapaneseCaption, ZebesPlanet];

    /// <summary>The caption plane with its first <paramref name="letters"/> letters shown, and the katakana caption when <paramref name="japanese"/>.</summary>
    internal static PicturedPlane Caption(InstalledSpriteSheets sheets, int letters, bool japanese)
    {
        SpriteSheet sheet = Ensure.NotNull(sheets).Get(SpriteSheetId.CeresCinematicPictures);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        for (int letter = 0; letter < letters; letter++)
            placed.Add(new(sheet.Frame(CaptionLetter(letter)), Corner(SpaceColonyCaption.Column(letter), SpaceColonyCaptionDefinitions.Row)));
        if (japanese)
            placed.Add(new(sheet.Frame(JapaneseCaption), Corner(SpaceColonyCaptionDefinitions.JapaneseColumn, SpaceColonyCaptionDefinitions.JapaneseRow)));
        return new PicturedPlane(new Extent16<TilemapCells>(PlaneCells, PlaneCells), placed.ToImmutable());
    }

    /// <summary>The Zebes reveal's plane: the planet once <paramref name="revealed"/>, nothing before.</summary>
    internal static PicturedPlane Zebes(InstalledSpriteSheets sheets, bool revealed) => new(new Extent16<TilemapCells>(PlaneCells, PlaneCells),
        revealed ? [new PlacedPlanePicture(Ensure.NotNull(sheets).Get(SpriteSheetId.CeresCinematicPictures).Frame(ZebesPlanet), default)] : []);

    /// <summary>The plane pixel of cell (<paramref name="column"/>, <paramref name="row"/>)'s upper-left corner.</summary>
    private static Position16<TilePlanePixels> Corner(int column, int row) => new(
        checked((ushort)(column * ScreenGeometry.BackgroundTileSizePixels)), checked((ushort)(row * ScreenGeometry.BackgroundTileSizePixels)));
}
