namespace SuperMetroid.Core.Assets;

/// <summary>Native visual identity for the opening-cinematic text caret.</summary>
internal static class IntroCaretSpriteDefinitions
{
    /// <summary>$8C:8D68, still caret used by the ordinary list at $8B:CBFB.</summary>
    internal const ushort Still = 0x8d68;
    /// <summary>The hardware OAM limit for an authored visual composition.</summary>
    internal const int MaximumParts = IntroCinematicSpriteCompiler.MaximumParts;

    /// <summary>Names from the mistaken version-one asset; only the first is caret art.</summary>
    internal static readonly string[] PreviousFrameNames =
        ["caret-still", "caret-blink-1", "caret-blink-2", "caret-blink-3"];

    /// <summary>$8C:8D68: the one-part visible caret; blink control selects visibility separately.</summary>
    internal static IntroCaretFrameDefinition Visible => new(Still, "caret-visible", 1);

    /// <summary>The singleton authored caret artwork entry; blink timing controls whether that sprite is drawn.</summary>
    internal static IReadOnlyList<IntroCaretFrameDefinition> Frames { get; } = new VisibleFrameList();

    /// <summary>Read-only one-item view used where the caret catalog is consumed as a frame collection.</summary>
    private sealed class VisibleFrameList : IReadOnlyList<IntroCaretFrameDefinition>
    {
        /// <summary>Gets the single visible artwork definition available to the caret.</summary>
        public int Count => 1;

        /// <summary>Gets the visible caret definition at index zero.</summary>
        /// <param name="index">Zero-based position in the one-entry collection.</param>
        /// <returns>The visible caret definition.</returns>
        /// <exception cref="IndexOutOfRangeException">The index is not zero.</exception>
        public IntroCaretFrameDefinition this[int index] => index == 0 ? Visible : throw new IndexOutOfRangeException();

        /// <summary>Enumerates the collection's one visible caret definition.</summary>
        /// <returns>An enumerator yielding <see cref="VisibleFrameList.this[int]"/> at index zero.</returns>
        public IEnumerator<IntroCaretFrameDefinition> GetEnumerator()
        {
            yield return Visible;
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Native spritemap identity and stock composition metadata for one authored caret visual.</summary>
/// <param name="Pointer">Bank-local pointer to the native caret spritemap in bank $8C.</param>
/// <param name="Name">Stable semantic identifier used to locate the caret visual in authored resources.</param>
/// <param name="StockPartCount">Number of OAM parts emitted by the original native spritemap.</param>
internal readonly record struct IntroCaretFrameDefinition(ushort Pointer, string Name, int StockPartCount);
