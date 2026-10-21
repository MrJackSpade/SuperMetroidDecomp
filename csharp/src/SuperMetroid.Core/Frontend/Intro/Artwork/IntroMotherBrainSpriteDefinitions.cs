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

    /// <summary>The pose of Mother Brain frame <paramref name="frame"/>, 0 through 2.</summary>
    internal static CinematicSpritePose FramePose(int frame) =>
        (uint)frame < 3 ? Frames[frame].Pose : throw new ArgumentOutOfRangeException(nameof(frame));
}
