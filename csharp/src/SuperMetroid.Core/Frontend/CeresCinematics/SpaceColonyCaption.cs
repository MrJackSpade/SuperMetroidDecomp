namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Lays out the English SPACE COLONY caption (<see cref="SpaceColonyCaptionDefinitions"/>): its visible letters left to
/// right, one letter's width apart, skipping the space.
/// </summary>
public static class SpaceColonyCaption
{
    /// <summary>The pixel column of visible letter <paramref name="index"/>'s left edge, the space's width skipped.</summary>
    public static int Left(int index) =>
        SpaceColonyCaptionDefinitions.FirstLetterLeft + TextIndex(index) * SpaceColonyCaptionDefinitions.LetterAdvance;

    /// <summary>Where visible letter <paramref name="index"/> lies in the caption's text.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No visible letter has the index.</exception>
    internal static int TextIndex(int index)
    {
        if ((uint)index >= SpaceColonyCaptionDefinitions.LetterCount) throw new ArgumentOutOfRangeException(nameof(index));
        return index + (index >= SpaceColonyCaptionDefinitions.Text.IndexOf(' ') ? 1 : 0);
    }
}
