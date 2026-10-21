namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Visual compositions selected by the scientist delivery and examination lists.</summary>
internal static class IntroScientistSpriteDefinitions
{
    /// <summary>
    /// Three two-part arrows, three six-part delivered poses, three one-part examined poses, then the one-part hold, which
    /// is the caret's composition. The published keys, including the legacy arrow and hold names, are retained.
    /// </summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("examined-loop-1", new(27), 2),
        new("examined-loop-2", new(28), 2),
        new("examined-loop-3", new(29), 2),
        new("delivered-baby-1", new(19), 6),
        new("delivered-baby-2", new(20), 6),
        new("delivered-baby-3", new(21), 6),
        new("examined-baby-1", new(22), 1),
        new("examined-baby-2", new(23), 1),
        new("examined-baby-3", new(24), 1),
        new("examined-baby-hold", new(25), 1),
    ];

    /// <summary>The pose of scientist frame <paramref name="index"/>, 0 through 9.</summary>
    internal static CinematicSpritePose FramePose(int index) =>
        (uint)index < 10 ? Frames[index].Pose : throw new ArgumentOutOfRangeException(nameof(index));
}
