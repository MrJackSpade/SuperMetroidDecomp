namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Lays out the English SPACE COLONY caption (<see cref="SpaceColonyCaptionDefinitions"/>): its visible letters left to
/// right, one cell each, skipping the undrawn space.
/// </summary>
internal static class SpaceColonyCaption
{
    /// <summary>Places a visible glyph from column ten, skipping the undrawn space cell.</summary>
    internal static SpaceColonyCaptionLetter Letter(int index)
    {
        if ((uint)index >= SpaceColonyCaptionDefinitions.LetterCount) throw new ArgumentOutOfRangeException(nameof(index));
        string text = SpaceColonyCaptionDefinitions.Text;
        int column = index + (index >= text.IndexOf(' ') ? 1 : 0);
        return new SpaceColonyCaptionLetter(SpaceColonyCaptionDefinitions.FirstColumn + column,
            Ensure.Listed(SpaceColonyCaptionDefinitions.Glyphs, text[column], nameof(index)));
    }
}
