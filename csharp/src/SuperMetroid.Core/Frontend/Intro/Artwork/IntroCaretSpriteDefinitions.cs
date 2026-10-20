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
    internal static IntroSpriteFrameDefinition Visible => new(Still, "caret-visible", 1);

    /// <summary>Exposes only the visible caret; blink frames are a visibility decision, not separate art.</summary>
    internal static IReadOnlyList<IntroSpriteFrameDefinition> Frames { get; } = [Visible];

}
