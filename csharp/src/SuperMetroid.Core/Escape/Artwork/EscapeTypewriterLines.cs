namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// Lays out the escape warnings (<see cref="EscapeTypewriterDefinitions"/>): how many lines each message types, and
/// each line's wording and the pixel of the layer its first character is typed at.
/// </summary>
internal static class EscapeTypewriterLines
{
    /// <summary>Ceres uses three warning lines while Zebes uses two.</summary>
    internal static int LineCount(EscapeTypewriterMessageId id) => Ensure.Listed(EscapeTypewriterDefinitions.LineCounts, id);

    /// <summary>Builds one selected warning line and where it starts from reviewed wording and line spacing.</summary>
    internal static EscapeTypewriterLine Line(EscapeTypewriterMessageId id, int index)
    {
        if ((uint)index >= LineCount(id)) throw new ArgumentOutOfRangeException(nameof(index));
        string text = Ensure.Listed(EscapeTypewriterDefinitions.LineTexts, new EscapeTypewriterLineKey(id, index));
        return new(new Position16<PlanePixels>(EscapeTypewriterDefinitions.FirstLeft,
            EscapeTypewriterDefinitions.FirstTop + index * EscapeTypewriterDefinitions.LineSpacing), text);
    }
}
