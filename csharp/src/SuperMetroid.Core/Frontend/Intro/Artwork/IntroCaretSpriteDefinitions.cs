namespace SuperMetroid.Core.Frontend.Intro.Artwork;

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

    /// <summary>Exposes only the visible caret; blink frames are a visibility decision, not separate art.</summary>
    internal static IReadOnlyList<IntroCaretFrameDefinition> Frames { get; } = new VisibleFrameList();

    /// <summary>Adapts the single visible native caret frame to the artwork loader's list contract.</summary>
    private sealed class VisibleFrameList : IReadOnlyList<IntroCaretFrameDefinition>
    {
        /// <summary>There is one caret composition despite four legacy frame names.</summary>
        public int Count => 1;
        /// <summary>Accepts only index zero because other blink states do not have compositions.</summary>
        public IntroCaretFrameDefinition this[int index] => index == 0 ? Visible : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Yields the one visible composition without allocating a stored array.</summary>
        public IEnumerator<IntroCaretFrameDefinition> GetEnumerator()
        {
            yield return Visible;
        }
        /// <summary>Provides the same single-frame sequence to non-generic list consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
