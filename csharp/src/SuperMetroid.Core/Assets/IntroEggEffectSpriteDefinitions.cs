namespace SuperMetroid.Core.Assets;

/// <summary>Six shell fragments and five slime-drop OAM frames from the SR388 scene.</summary>
internal static class IntroEggEffectSpriteDefinitions
{
    /// <summary>$8C:8F7E, first shell-fragment one-part composition.</summary>
    internal const ushort Start = 0x8f7e;
    /// <summary>$8C:8FCB, exclusive end after eleven consecutive seven-byte records.</summary>
    internal const ushort End = 0x8fcb;
    internal const int StockPartCount = 1;

    private static readonly IntroEggEffectSpriteFrameDefinition[] frames =
    [
        new(0x8f7e, "fragment-0"),
        new(0x8f85, "fragment-1"),
        new(0x8f8c, "fragment-2"),
        new(0x8f93, "fragment-3"),
        new(0x8f9a, "fragment-4"),
        new(0x8fa1, "fragment-5"),
        new(0x8fa8, "slime-moving"),
        new(0x8faf, "slime-impact-0"),
        new(0x8fb6, "slime-impact-1"),
        new(0x8fbd, "slime-impact-2"),
        new(0x8fc4, "slime-impact-3"),
    ];

    internal static ReadOnlySpan<IntroEggEffectSpriteFrameDefinition> Frames => frames;
}

internal readonly record struct IntroEggEffectSpriteFrameDefinition(ushort Pointer, string Name);
