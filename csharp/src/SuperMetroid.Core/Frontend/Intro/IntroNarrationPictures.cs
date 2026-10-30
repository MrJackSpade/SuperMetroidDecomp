using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening narration's pictures (#1467): the first card, the final line, and each letter of the narration's alphabet
/// in each colour variant its glow shows, which the import composes once from the narration font under the cinematic's
/// palette. The cinematic places them on the narration plane its text state shows.
/// </summary>
public static class IntroNarrationPictures
{
    /// <summary>The name of the first card's picture.</summary>
    public const string FirstCard = "first-card";

    /// <summary>The name of the final line's picture.</summary>
    public const string FinalLine = "final-line";

    /// <summary>The colour variants a letter shows: the variant it is typed in and each its glow gives it.</summary>
    public const int LetterColourVariants = CinematicTextGlowDefinitions.FinalPalette + 1;

    /// <summary>The cells along each side of the narration plane.</summary>
    internal const int PlaneCells = IntroCinematicDefinitions.Tilemaps.TilemapWidth;

    /// <summary>The plane row the final line's picture starts at.</summary>
    internal const int FinalLineRow = IntroCinematicDefinitions.Text.FinalLineFirstCell / IntroCinematicDefinitions.Tilemaps.TilemapWidth;

    /// <summary>The name of <paramref name="letter"/>'s picture in colour variant <paramref name="colourVariant"/>.</summary>
    internal static string Letter(char letter, int colourVariant) => $"letter-{(int)letter:x2}-{colourVariant}";

    /// <summary>Each letter the narration types: its alphabet without the space, which types none.</summary>
    internal static IEnumerable<char> Letters => IntroNarrationDefinitions.Alphabet.Where(static letter => letter != ' ');

    /// <summary>The name of every picture the sheet holds.</summary>
    public static IEnumerable<string> FrameNames =>
    [
        FirstCard, FinalLine,
        .. Letters.SelectMany(static letter => Enumerable.Range(0, LetterColourVariants).Select(variant => Letter(letter, variant))),
    ];

    /// <summary>The plane showing the first card.</summary>
    internal static PicturedPlane FirstCardPlane(InstalledSpriteSheets sheets) =>
        Plane([new PlacedPlanePicture(Sheet(sheets).Frame(FirstCard), default)]);

    /// <summary>The plane showing the narration pages: the final line, and each letter <paramref name="text"/> shows at its cell.</summary>
    internal static PicturedPlane PagesPlane(InstalledSpriteSheets sheets, IntroNarrationText text)
    {
        Ensure.NotNull(text);
        SpriteSheet sheet = Sheet(sheets);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        placed.Add(new(sheet.Frame(FinalLine), At(0, FinalLineRow)));
        for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
        {
            for (int column = 0; column < IntroNarrationText.RowCells; column++)
            {
                IntroTextCell cell = text.ShownAt(column, row);
                if (cell.Letter is { } letter)
                    placed.Add(new(sheet.Frame(Letter(letter, cell.ColourVariant)), At(column, row)));
            }
        }
        return Plane(placed.ToImmutable());
    }

    /// <summary>The plane position of the cell at <paramref name="column"/> of <paramref name="row"/>.</summary>
    private static Position16<TilePlanePixels> At(int column, int row) =>
        new(checked((ushort)(column * ScreenGeometry.BackgroundTileSizePixels)), checked((ushort)(row * ScreenGeometry.BackgroundTileSizePixels)));

    /// <summary>The installed narration picture sheet.</summary>
    private static SpriteSheet Sheet(InstalledSpriteSheets sheets) => Ensure.NotNull(sheets).Get(SpriteSheetId.IntroNarrationPictures);

    /// <summary>A narration plane holding <paramref name="placed"/>.</summary>
    private static PicturedPlane Plane(ImmutableArray<PlacedPlanePicture> placed) =>
        new(new Extent16<TilemapCells>(PlaneCells, PlaneCells), placed);
}
