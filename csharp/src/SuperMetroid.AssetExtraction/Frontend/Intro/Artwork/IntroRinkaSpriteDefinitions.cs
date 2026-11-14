using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The three four-quadrant Rinka frames selected by the intro list.</summary>
internal static class IntroRinkaSpriteDefinitions
{
    /// <summary>Number of OAM parts in each stock Rinka composition.</summary>
    internal const int StockPartCount = 4;

    /// <summary>The three intro Rinka compositions.</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("rinka-0", CinematicSpritePose.Rinka0, 4),
        new("rinka-1", CinematicSpritePose.Rinka1, 4),
        new("rinka-2", CinematicSpritePose.Rinka2, 4),
    ];
}
