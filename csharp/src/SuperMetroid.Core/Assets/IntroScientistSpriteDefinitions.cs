namespace SuperMetroid.Core.Assets;

/// <summary>Visual compositions selected by the scientist delivery and examination lists.</summary>
internal static class IntroScientistSpriteDefinitions
{
    /// <summary>$8C:8CCF, first shared scientist-scene baby composition.</summary>
    internal const ushort Start = 0x8ccf;
    /// <summary>$8C:8D6F, exclusive end of ten consecutive compositions.</summary>
    internal const ushort End = 0x8d6f;

    private static readonly IntroScientistSpriteFrameDefinition[] frames =
    [
        new(0x8ccf, "examined-loop-1", 2),
        new(0x8cdb, "examined-loop-2", 2),
        new(0x8ce7, "examined-loop-3", 2),
        new(0x8cf3, "delivered-baby-1", 6),
        new(0x8d13, "delivered-baby-2", 6),
        new(0x8d33, "delivered-baby-3", 6),
        new(0x8d53, "examined-baby-1", 1),
        new(0x8d5a, "examined-baby-2", 1),
        new(0x8d61, "examined-baby-3", 1),
        new(0x8d68, "examined-baby-hold", 1),
    ];

    internal static ReadOnlySpan<IntroScientistSpriteFrameDefinition> Frames => frames;
}

internal readonly record struct IntroScientistSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
