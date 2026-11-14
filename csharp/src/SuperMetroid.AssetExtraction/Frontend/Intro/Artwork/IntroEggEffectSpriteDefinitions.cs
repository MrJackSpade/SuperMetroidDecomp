using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Six shell fragments and five slime-drop OAM frames from the SR388 scene.</summary>
internal static class IntroEggEffectSpriteDefinitions
{
    /// <summary>Every shell-fragment and slime-drop composition contains one OAM part.</summary>
    internal const int StockPartCount = 1;

    /// <summary>Ordered six shell fragments, one moving slime frame, and four impact frames.</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("fragment-0", CinematicSpritePose.EggFragment0, 1),
        new("fragment-1", CinematicSpritePose.EggFragment1, 1),
        new("fragment-2", CinematicSpritePose.EggFragment2, 1),
        new("fragment-3", CinematicSpritePose.EggFragment3, 1),
        new("fragment-4", CinematicSpritePose.EggFragment4, 1),
        new("fragment-5", CinematicSpritePose.EggFragment5, 1),
        new("slime-moving", CinematicSpritePose.SlimeMoving, 1),
        new("slime-impact-0", CinematicSpritePose.SlimeImpact0, 1),
        new("slime-impact-1", CinematicSpritePose.SlimeImpact1, 1),
        new("slime-impact-2", CinematicSpritePose.SlimeImpact2, 1),
        new("slime-impact-3", CinematicSpritePose.SlimeImpact3, 1),
    ];
}
