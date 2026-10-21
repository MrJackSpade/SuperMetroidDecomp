using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The compositions the four $8B:EE5D..EE9A logo lists show.</summary>
public static class EndingLogoSpriteDefinitions
{
    /// <summary>The upper and lower S, then three right-wrap and three left-wrap circle stages.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("s-upper", new(202), 14),
        new("s-lower", new(203), 14),
        new("circle-right-1", new(204), 12),
        new("circle-right-2", new(205), 18),
        new("circle-right-3", new(206), 25),
        new("circle-left-1", new(207), 12),
        new("circle-left-2", new(208), 18),
        new("circle-left-3", new(209), 25),
    ];
}
