namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Six shell fragments and five slime-drop OAM frames from the SR388 scene.</summary>
internal static class IntroEggEffectSpriteDefinitions
{
    /// <summary>Every shell-fragment and slime-drop composition contains one OAM part.</summary>
    internal const int StockPartCount = 1;

    /// <summary>Ordered six shell fragments, one moving slime frame, and four impact frames.</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("fragment-0", new(51), 1),
        new("fragment-1", new(52), 1),
        new("fragment-2", new(53), 1),
        new("fragment-3", new(54), 1),
        new("fragment-4", new(55), 1),
        new("fragment-5", new(56), 1),
        new("slime-moving", new(57), 1),
        new("slime-impact-0", new(58), 1),
        new("slime-impact-1", new(59), 1),
        new("slime-impact-2", new(60), 1),
        new("slime-impact-3", new(61), 1),
    ];

    /// <summary>The pose of effect frame <paramref name="frame"/>, 0 through 10.</summary>
    internal static CinematicSpritePose FramePose(int frame) =>
        (uint)frame < 11 ? Frames[frame].Pose : throw new ArgumentOutOfRangeException(nameof(frame));
}
