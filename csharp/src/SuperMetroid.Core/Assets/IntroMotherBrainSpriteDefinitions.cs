namespace SuperMetroid.Core.Assets;

/// <summary>Three visual frames selected by the intro Mother Brain's compiled lists.</summary>
internal static class IntroMotherBrainSpriteDefinitions
{
    /// <summary>$8C:8C00, first nine-part Mother Brain frame.</summary>
    internal const ushort FrameZero = 0x8c00;
    /// <summary>$8C:8C2F, second nine-part Mother Brain frame.</summary>
    internal const ushort FrameOne = 0x8c2f;
    /// <summary>$8C:8C5E, third nine-part Mother Brain frame.</summary>
    internal const ushort FrameTwo = 0x8c5e;
    /// <summary>Every retail frame has nine OAM parts; $8C:8C8D is unrelated.</summary>
    internal const int StockPartCount = 9;

    private static readonly IntroMotherBrainSpriteFrameDefinition[] frameDefinitions =
    [
        new(FrameZero, "mother-brain-frame-0"),
        new(FrameOne, "mother-brain-frame-1"),
        new(FrameTwo, "mother-brain-frame-2"),
    ];

    internal static ReadOnlySpan<IntroMotherBrainSpriteFrameDefinition> Frames => frameDefinitions;
}

internal readonly record struct IntroMotherBrainSpriteFrameDefinition(ushort Pointer, string Name);
