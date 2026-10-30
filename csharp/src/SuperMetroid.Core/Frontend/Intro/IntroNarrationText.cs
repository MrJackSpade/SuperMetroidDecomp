using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's narration text as display-model text (#1467). Until the first illustrated page the narration plane shows
/// the installed first card. From then on it shows the narration's letters at their cells over the installed final line,
/// each letter in the colour variant its glow last gave it.
/// </summary>
/// <remarks>
/// The narration types letters and its glows recolour cells in staging; the cinematic shows the staged rows when it
/// publishes them, once each update after the objects run and whenever it clears the page, as the cartridge uploads its
/// staging map to the narration plane. Letters only ever lie in the text rows between the top border and the final line.
/// </remarks>
internal sealed class IntroNarrationText
{
    /// <summary>The first cell of the text rows.</summary>
    private const int FirstTextCell = IntroCinematicDefinitions.Text.GameplayBlankStartIndex;

    /// <summary>The cells of the text rows.</summary>
    private const int TextCells = IntroCinematicDefinitions.Text.GameplayBlankWordCount;

    /// <summary>The cells along a text row.</summary>
    internal const int RowCells = IntroCinematicDefinitions.Tilemaps.TilemapWidth;

    /// <summary>The text rows' staged cells, which the narration types into and its glows recolour.</summary>
    private readonly IntroTextCell[] staging = new IntroTextCell[TextCells];

    /// <summary>The text rows' shown cells.</summary>
    private readonly IntroTextCell[] shown = new IntroTextCell[TextCells];

    /// <summary>Whether the plane shows the installed first card rather than the narration pages.</summary>
    internal bool FirstCardShown { get; private set; } = true;

    /// <summary>The first row of the text rows.</summary>
    internal static int FirstTextRow => FirstTextCell / RowCells;

    /// <summary>The rows the text rows span.</summary>
    internal static int TextRows => TextCells / RowCells;

    /// <summary>The shown cell at <paramref name="column"/> of text row <paramref name="row"/>, counted from the plane's top.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the text rows.</exception>
    internal IntroTextCell ShownAt(int column, int row) => shown[TextIndex(column, row)];

    /// <summary>Replaces the first card with the narration pages, every text cell blank and shown blank.</summary>
    internal void ShowPages()
    {
        FirstCardShown = false;
        Array.Clear(staging);
        Array.Clear(shown);
    }

    /// <summary>Types <paramref name="character"/> into its cell, in colour variant zero; a space blanks the cell.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The character's cell lies outside the text rows.</exception>
    internal void Type(IntroNarrationCharacter character) =>
        staging[TextIndex(character.Column, character.Row)] = new(character.IsSpace ? null : character.Character, 0);

    /// <summary>Gives the staged cell at <paramref name="cell"/> colour variant <paramref name="colourVariant"/>, keeping what it shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the text rows.</exception>
    internal void Recolour(Position16<TilemapCells> cell, int colourVariant)
    {
        int index = TextIndex(cell.X, cell.Y);
        staging[index] = staging[index] with { ColourVariant = colourVariant };
    }

    /// <summary>Blanks every staged text cell.</summary>
    internal void ClearText() => Array.Clear(staging);

    /// <summary>Shows the staged text rows.</summary>
    internal void Publish() => staging.CopyTo(shown, 0);

    /// <summary>The index in the text rows of the cell at <paramref name="column"/> of row <paramref name="row"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the text rows.</exception>
    private static int TextIndex(int column, int row) =>
        (Ensure.BetweenInclusive(FirstTextRow, FirstTextRow + TextRows - 1, row) - FirstTextRow) * RowCells +
        Ensure.BetweenInclusive(0, RowCells - 1, column);
}
