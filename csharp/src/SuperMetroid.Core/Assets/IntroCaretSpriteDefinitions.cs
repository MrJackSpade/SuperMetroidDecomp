namespace SuperMetroid.Core.Assets;

/// <summary>Native visual identity for the opening-cinematic text caret.</summary>
internal static class IntroCaretSpriteDefinitions
{
    /// <summary>$8C:8D68, still caret used by the ordinary list at $8B:CBFB.</summary>
    internal const ushort Still = 0x8d68;
    /// <summary>SNES OBJ tile numbers span two 16-column pages in the intro sheet.</summary>
    internal const int TileColumns = 16;
    /// <summary>The two OBJ pages total 32 rows of eight-pixel tiles.</summary>
    internal const int TileRows = 32;
    /// <summary>The hardware OAM limit for an authored visual composition.</summary>
    internal const int MaximumParts = 128;

    /// <summary>Names from the mistaken version-one asset; only the first is caret art.</summary>
    internal static readonly string[] PreviousFrameNames =
        ["caret-still", "caret-blink-1", "caret-blink-2", "caret-blink-3"];

    private static readonly IntroCaretFrameDefinition[] frameDefinitions =
    [
        new(Still, "caret-visible", 1),
    ];

    internal static ReadOnlySpan<IntroCaretFrameDefinition> Frames => frameDefinitions;
}

internal readonly record struct IntroCaretFrameDefinition(ushort Pointer, string Name, int StockPartCount);
