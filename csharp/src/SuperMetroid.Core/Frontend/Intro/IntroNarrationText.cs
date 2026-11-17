using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening's narration text as a typed text scene (#1467, #1555): the letter typed at each place of the narration's
/// fixed-width text rows, each in the colours of the glow step it last showed. The narration types letters into it and its
/// glows recolour them; the cinematic shows it as it is, each letter at its screen pixel over the installed final line.
/// </summary>
/// <remarks>
/// Letters only ever lie in the text rows between the top border and the final line. The
/// narration's typing and glows run before the frame is shown in every update that changes them, so what the text holds
/// is what the frame shows.
/// </remarks>
internal sealed class IntroNarrationText
{
    /// <summary>The first text row, counted in glyphs from the narration's top.</summary>
    internal const int FirstTextRow = IntroNarrationDefinitions.FirstTextRow;

    /// <summary>The text rows, from <see cref="FirstTextRow"/> down to the final line.</summary>
    internal const int TextRows = 20;

    /// <summary>The glyph columns along a text row: as many glyphs as fit across the screen.</summary>
    internal const int GlyphColumns = ScreenGeometry.ScreenWidthPixels / ScreenGeometry.BackgroundCellSizePixels;

    /// <summary>The letter and colour variant at each place of the text rows, row by row.</summary>
    private readonly IntroTextCell[] places = new IntroTextCell[TextRows * GlyphColumns];

    /// <summary>The letter at <paramref name="column"/> of text row <paramref name="row"/>, counted from the narration's top.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place lies outside the text rows.</exception>
    internal IntroTextCell ShownAt(int column, int row) => places[PlaceIndex(column, row)];

    /// <summary>Types <paramref name="character"/> at its place, in the typed colours; a space blanks the place.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The character's place lies outside the text rows.</exception>
    internal void Type(IntroNarrationCharacter character) =>
        places[PlaceIndex(character.Column, character.Row)] = new(character.IsSpace ? null : character.Character, IntroTextGlowStep.Typed);

    /// <summary>Gives the place at <paramref name="column"/> of row <paramref name="row"/> the colours of glow step <paramref name="glow"/>, keeping its letter.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place lies outside the text rows.</exception>
    internal void Recolour(int column, int row, IntroTextGlowStep glow)
    {
        int index = PlaceIndex(column, row);
        places[index] = places[index] with { Glow = glow };
    }

    /// <summary>Blanks every place of the text rows.</summary>
    internal void ClearText() => Array.Clear(places);

    /// <summary>The index of the place at <paramref name="column"/> of row <paramref name="row"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place lies outside the text rows.</exception>
    private static int PlaceIndex(int column, int row) =>
        (Ensure.BetweenInclusive(FirstTextRow, FirstTextRow + TextRows - 1, row) - FirstTextRow) * GlyphColumns +
        Ensure.BetweenInclusive(0, GlyphColumns - 1, column);
}
