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
        new("rinka-dim", CinematicSpritePose.RinkaDim, 4),
        new("rinka-glowing", CinematicSpritePose.RinkaGlowing, 4),
        new("rinka-bright", CinematicSpritePose.RinkaBright, 4),
    ];
}
