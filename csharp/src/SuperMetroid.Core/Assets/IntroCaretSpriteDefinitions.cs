namespace SuperMetroid.Core.Assets;

/// <summary>Native visual identities for the four opening-cinematic caret frames.</summary>
internal static class IntroCaretSpriteDefinitions
{
    /// <summary>$8C:8D68, still caret used by the ordinary list at $8B:CBFB.</summary>
    internal const ushort Still = 0x8d68;
    /// <summary>$8C:8CCF, first visible frame of the blinking list at $8B:CC03.</summary>
    internal const ushort BlinkOne = 0x8ccf;
    /// <summary>$8C:8CDB, second visible frame of the blinking list at $8B:CC03.</summary>
    internal const ushort BlinkTwo = 0x8cdb;
    /// <summary>$8C:8CE7, third visible frame of the blinking list at $8B:CC03.</summary>
    internal const ushort BlinkThree = 0x8ce7;
    /// <summary>SNES OBJ tile numbers span two 16-column pages in the intro sheet.</summary>
    internal const int TileColumns = 16;
    /// <summary>The two OBJ pages total 32 rows of eight-pixel tiles.</summary>
    internal const int TileRows = 32;
    /// <summary>The hardware OAM limit for an authored visual composition.</summary>
    internal const int MaximumParts = 128;

    private static readonly IntroCaretFrameDefinition[] frameDefinitions =
    [
        new(Still, "caret-still", 1),
        new(BlinkOne, "caret-blink-1", 2),
        new(BlinkTwo, "caret-blink-2", 2),
        new(BlinkThree, "caret-blink-3", 2),
    ];

    internal static ReadOnlySpan<IntroCaretFrameDefinition> Frames => frameDefinitions;
}

internal readonly record struct IntroCaretFrameDefinition(ushort Pointer, string Name, int StockPartCount);
