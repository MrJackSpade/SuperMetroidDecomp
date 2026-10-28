namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Three visual frames selected by the intro Mother Brain's compiled lists.</summary>
internal static class IntroMotherBrainSpriteDefinitions
{
    /// <summary>Every retail frame has nine OAM parts.</summary>
    internal const int StockPartCount = 9;

    /// <summary>The intro's three Mother Brain frames.</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("mother-brain-frame-0", new(0), 9),
        new("mother-brain-frame-1", new(1), 9),
        new("mother-brain-frame-2", new(2), 9),
    ];
}
