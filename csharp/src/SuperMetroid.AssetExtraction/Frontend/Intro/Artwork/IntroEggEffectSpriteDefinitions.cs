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
        new("shell-upper-left", CinematicSpritePose.EggShellUpperLeft, 1),
        new("shell-upper-right", CinematicSpritePose.EggShellUpperRight, 1),
        new("shell-left", CinematicSpritePose.EggShellLeft, 1),
        new("shell-centre", CinematicSpritePose.EggShellCentre, 1),
        new("shell-right", CinematicSpritePose.EggShellRight, 1),
        new("shell-lower", CinematicSpritePose.EggShellLower, 1),
        new("slime-moving", CinematicSpritePose.SlimeMoving, 1),
        new("slime-impact-0", CinematicSpritePose.SlimeImpact0, 1),
        new("slime-impact-1", CinematicSpritePose.SlimeImpact1, 1),
        new("slime-impact-2", CinematicSpritePose.SlimeImpact2, 1),
        new("slime-impact-3", CinematicSpritePose.SlimeImpact3, 1),
    ];
}
