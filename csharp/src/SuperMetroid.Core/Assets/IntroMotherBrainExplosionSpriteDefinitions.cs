namespace SuperMetroid.Core.Assets;

/// <summary>The twelve OAM compositions selected by the fourth-hit explosion loops.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitions
{
    /// <summary>$8C:97F7, first small-explosion frame; the last frame ends at $8C:985D.</summary>
    internal const ushort SmallStart = 0x97f7;
    /// <summary>$8C:985D, first large-explosion frame; the last frame ends at $8C:98D2.</summary>
    internal const ushort BigStart = 0x985d;
    /// <summary>$8C:98D2, exclusive end of the twelve consecutive composition records.</summary>
    internal const ushort End = 0x98d2;

    private static readonly IntroMotherBrainExplosionSpriteFrameDefinition[] frames =
    [
        new(0x97f7, "small-explosion-0", 1),
        new(0x97fe, "small-explosion-1", 1),
        new(0x9805, "small-explosion-2", 4),
        new(0x981b, "small-explosion-3", 4),
        new(0x9831, "small-explosion-4", 4),
        new(0x9847, "small-explosion-5", 4),
        new(0x985d, "big-explosion-0", 1),
        new(0x9864, "big-explosion-1", 4),
        new(0x987a, "big-explosion-2", 4),
        new(0x9890, "big-explosion-3", 4),
        new(0x98a6, "big-explosion-4", 4),
        new(0x98bc, "big-explosion-5", 4),
    ];

    internal static ReadOnlySpan<IntroMotherBrainExplosionSpriteFrameDefinition> Frames => frames;
}

internal readonly record struct IntroMotherBrainExplosionSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
