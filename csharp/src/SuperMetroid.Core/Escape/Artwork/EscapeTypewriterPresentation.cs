namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable escape-warning text and visual line placement.</summary>
public sealed class EscapeTypewriterPresentation
{
    /// <summary>Loaded Ceres warning lines and where each starts.</summary>
    private readonly EscapeTypewriterMessage ceres;
    /// <summary>Loaded Zebes time-bomb warning lines and where each starts.</summary>
    private readonly EscapeTypewriterMessage zebes;

    /// <summary>Retains the two messages the import pipeline validated.</summary>
    internal EscapeTypewriterPresentation(Dictionary<EscapeTypewriterMessageId, EscapeTypewriterMessage> messages)
    {
        ceres = messages[EscapeTypewriterMessageId.Ceres];
        zebes = messages[EscapeTypewriterMessageId.Zebes];
    }

    /// <summary>Returns the installed warning text and placement for one escape scenario.</summary>
    /// <param name="id">Ceres or Zebes message identity; None does not denote an installed message.</param>
    /// <returns>The retained message with its read-only ordered line sequence and source identity.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is None or another undefined message value.</exception>
    public EscapeTypewriterMessage Get(EscapeTypewriterMessageId id) => id switch
    {
        EscapeTypewriterMessageId.Ceres => ceres,
        EscapeTypewriterMessageId.Zebes => zebes,
        EscapeTypewriterMessageId.None => throw new ArgumentOutOfRangeException(nameof(id), id,
            "No escape warning of this kind is installed."),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id,
            "No escape warning of this kind is installed."),
    };

    /// <summary>Reads stock lines from compiled definitions and stores only authored changes by line index.</summary>
    internal sealed class LineSequence(EscapeTypewriterMessageId id, int count,
        Dictionary<int, EscapeTypewriterLine> differences) : IReadOnlyList<EscapeTypewriterLine>
    {
        /// <summary>Edited or added lines that differ from the selected warning message.</summary>
        internal readonly Dictionary<int, EscapeTypewriterLine> overrides = differences;
        /// <summary>Number of lines selected by the editable document for this warning.</summary>
        public int Count => count;
        /// <summary>Reads an authored override when present, otherwise the compiled line.</summary>
        public EscapeTypewriterLine this[int index] => (uint)index < Count
            ? overrides.TryGetValue(index, out EscapeTypewriterLine? line) ? line : EscapeTypewriterLines.Line(id, index)
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Yields warning lines in typewriter order, resolving stock lines lazily.</summary>
        public IEnumerator<EscapeTypewriterLine> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same ordered line sequence through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
