using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening narration's pictures (#1467): the first card, the final line, and each letter of the narration's alphabet,
/// whose colour variants its glow shows are shades (#1512), which the import composes once from the narration font under the cinematic's
/// palette. The cinematic places them at the screen pixels its text scene shows them at.
/// </summary>
public static class IntroNarrationPictures
{
    /// <summary>The name of the first card's picture.</summary>
    public const string FirstCard = "first-card";

    /// <summary>The name of the final line's picture.</summary>
    public const string FinalLine = "final-line";

    /// <summary>The cells along each side of the narration plane.</summary>
    internal const int PlaneCells = IntroCinematicDefinitions.Planes.Width;

    /// <summary>The row, counted in glyphs from the narration's top, the final line's picture starts at.</summary>
    internal const int FinalLineRow = IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows;

    /// <summary>The side of one narration glyph in pixels.</summary>
    private const int GlyphPixels = ScreenGeometry.BackgroundCellSizePixels;

    /// <summary>
    /// Pixels the narration pages show above the screen's top: the pages' first row of glyphs lies one row up from the
    /// screen's top, the same distance the flashback rooms lie up.
    /// </summary>
    private const int PagesRaise = IntroCinematicState.GameplayFlashbackVerticalScroll;

    /// <summary>The name of <paramref name="letter"/>'s picture, which shows each colour variant as a shade (#1512).</summary>
    internal static string Letter(char letter) => $"letter-{(int)letter:x2}";

    /// <summary>
    /// The plane showing the first card in <paramref name="shading"/> (#1512): the scene's group shades, as the card's colours
    /// lie in several groups and in colours no group moves.
    /// </summary>
    internal static PicturedPlane FirstCardPlane(InstalledSpriteSheets sheets, PictureShading shading) =>
        Plane([new PlacedPlanePicture(Sheet(sheets).Frame(FirstCard), default, Ensure.NotNull(shading))]);

    /// <summary>
    /// The screen of the narration pages (#1512, #1555): the final line in <paramref name="finalLine"/>, as its colours lie
    /// in several groups and in colours no group moves, and each letter <paramref name="text"/> holds at its screen pixel in
    /// the shade of its colour variant, as <paramref name="shown"/> shows its own colours.
    /// </summary>
    internal static PicturedPlane PagesPlane(InstalledSpriteSheets sheets, IntroNarrationText text, PictureShading finalLine,
        Func<PictureShading, PictureShading> shown)
    {
        Ensure.NotNull(text);
        SpriteSheet sheet = Sheet(sheets);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        Ensure.NotNull(shown);
        placed.Add(new(sheet.Frame(FinalLine), At(0, FinalLineRow), Ensure.NotNull(finalLine)));
        for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
        {
            for (int column = 0; column < IntroNarrationText.RowCells; column++)
            {
                IntroTextCell cell = text.ShownAt(column, row);
                if (cell.Letter is { } letter)
                    placed.Add(new(sheet.Frame(Letter(letter)), At(column, row), shown(PictureShading.Shaded(IntroShades.Glow(cell.ColourVariant)))));
            }
        }
        return Plane(placed.ToImmutable());
    }

    /// <summary>The screen pixel of the glyph at <paramref name="column"/> of <paramref name="row"/>, counted from the narration's top.</summary>
    private static Offset16<PlanePixels> At(int column, int row) =>
        new(column * GlyphPixels, row * GlyphPixels - PagesRaise);

    /// <summary>The installed narration picture sheet.</summary>
    private static SpriteSheet Sheet(InstalledSpriteSheets sheets) => Ensure.NotNull(sheets).Get(SpriteSheetId.IntroNarrationPictures);

    /// <summary>A narration plane holding <paramref name="placed"/>.</summary>
    private static PicturedPlane Plane(ImmutableArray<PlacedPlanePicture> placed) => new(placed);
}
