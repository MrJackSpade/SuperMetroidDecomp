namespace SuperMetroid.Core.Assets;

/// <summary>Visual OAM compositions selected by the SR388 egg and confused-baby lists.</summary>
internal static class IntroDiscoveryActorSpriteDefinitions
{
    /// <summary>$8C:8D6F, first egg composition.</summary>
    internal const ushort EggStart = 0x8d6f;
    /// <summary>$8C:8F7E, exclusive end of the sixteen consecutive egg compositions.</summary>
    internal const ushort EggEnd = 0x8f7e;
    /// <summary>$8C:8FCB, first confused-baby composition.</summary>
    internal const ushort BabyStart = 0x8fcb;
    /// <summary>$8C:8FE0, exclusive end of the three small confused-baby compositions.</summary>
    internal const ushort BabySmallEnd = 0x8fe0;
    /// <summary>$8C:909D, the large hatched-baby composition.</summary>
    internal const ushort BabyLarge = 0x909d;
    /// <summary>$8C:90FE, exclusive end of the large hatched-baby composition.</summary>
    internal const ushort BabyEnd = 0x90fe;

    private static readonly IntroDiscoveryActorSpriteFrameDefinition[] frames =
    [
        new(0x8d6f, "egg-intact", 6),
        new(0x8d8f, "egg-crack-1", 9),
        new(0x8dbe, "egg-crack-2", 9),
        new(0x8ded, "egg-crack-3", 9),
        new(0x8e1c, "egg-crack-4", 9),
        new(0x8e4b, "egg-crack-5", 9),
        new(0x8e7a, "egg-crack-6", 9),
        new(0x8ea9, "egg-crack-7", 9),
        new(0x8ed8, "egg-hatched", 9),
        new(0x8f07, "egg-remnant-1", 3),
        new(0x8f18, "egg-remnant-2", 3),
        new(0x8f29, "egg-remnant-3", 3),
        new(0x8f3a, "egg-remnant-4", 3),
        new(0x8f4b, "egg-remnant-5", 3),
        new(0x8f5c, "egg-remnant-6", 3),
        new(0x8f6d, "egg-remnant-7", 3),
        new(0x8fcb, "confused-baby-1", 1),
        new(0x8fd2, "confused-baby-2", 1),
        new(0x8fd9, "confused-baby-3", 1),
        new(0x909d, "hatched-baby", 19),
    ];

    internal static ReadOnlySpan<IntroDiscoveryActorSpriteFrameDefinition> Frames => frames;
}

internal readonly record struct IntroDiscoveryActorSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
