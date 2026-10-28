namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// Lays out the escape warnings (<see cref="EscapeTypewriterDefinitions"/>): how many lines each program types, and
/// each line's wording and first overlay cell.
/// </summary>
internal static class EscapeTypewriterLines
{
    /// <summary>Ceres uses three warning lines while Zebes uses two.</summary>
    internal static int LineCount(EscapeTypewriterProgramId id) => Ensure.Listed(EscapeTypewriterDefinitions.LineCounts, id);

    /// <summary>Builds one selected warning line and its first overlay cell from reviewed wording and line spacing.</summary>
    internal static EscapeTypewriterLine Line(EscapeTypewriterProgramId id, int index)
    {
        if ((uint)index >= LineCount(id)) throw new ArgumentOutOfRangeException(nameof(index));
        string text = Ensure.Listed(EscapeTypewriterDefinitions.LineTexts, new EscapeTypewriterLineKey(id, index), nameof(index));
        return new((EscapeTypewriterDefinitions.FirstRow + index * EscapeTypewriterDefinitions.LineRowStep) *
            EscapeTypewriterDefinitions.MaximumLineLength + EscapeTypewriterDefinitions.FirstColumn, text);
    }
}
