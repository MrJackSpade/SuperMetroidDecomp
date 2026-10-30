namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Lays out the English SPACE COLONY caption (<see cref="SpaceColonyCaptionDefinitions"/>): its visible letters left to
/// right, one cell each, skipping the undrawn space.
/// </summary>
public static class SpaceColonyCaption
{
    /// <summary>The column visible letter <paramref name="index"/> is drawn at, skipping the undrawn space cell.</summary>
    public static int Column(int index) => SpaceColonyCaptionDefinitions.FirstColumn + TextIndex(index);

    /// <summary>The caption character visible letter <paramref name="index"/> draws.</summary>
    public static char Character(int index) => SpaceColonyCaptionDefinitions.Text[TextIndex(index)];

    /// <summary>Where visible letter <paramref name="index"/> lies in the caption's text.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No visible letter has the index.</exception>
    private static int TextIndex(int index)
    {
        if ((uint)index >= SpaceColonyCaptionDefinitions.LetterCount) throw new ArgumentOutOfRangeException(nameof(index));
        return index + (index >= SpaceColonyCaptionDefinitions.Text.IndexOf(' ') ? 1 : 0);
    }
}
