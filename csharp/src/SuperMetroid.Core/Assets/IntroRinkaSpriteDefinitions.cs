namespace SuperMetroid.Core.Assets;

/// <summary>The three four-quadrant Rinka frames selected by the intro list.</summary>
internal static class IntroRinkaSpriteDefinitions
{
    /// <summary>$8C:8C8D, first intro Rinka composition.</summary>
    internal const ushort First = 0x8c8d;
    /// <summary>$8C:8CCF, exclusive end after three consecutive 22-byte records.</summary>
    internal const ushort End = 0x8ccf;

    private static readonly IntroRinkaSpriteFrameDefinition[] frames =
    [
        new(0x8c8d, "rinka-0"),
        new(0x8ca3, "rinka-1"),
        new(0x8cb9, "rinka-2"),
    ];

    internal static ReadOnlySpan<IntroRinkaSpriteFrameDefinition> Frames => frames;
    internal const int StockPartCount = 4;
}

internal readonly record struct IntroRinkaSpriteFrameDefinition(ushort Pointer, string Name);
