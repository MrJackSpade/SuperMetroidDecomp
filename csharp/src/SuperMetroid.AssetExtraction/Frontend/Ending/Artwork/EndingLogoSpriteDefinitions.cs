using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The compositions the four $8B:EE5D..EE9A logo lists show.</summary>
public static partial class EndingLogoSpriteDefinitions
{
    /// <summary>The upper and lower S, then three right-wrap and three left-wrap circle stages.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("s-upper", CinematicSpritePose.LogoSUpper, 14),
        new("s-lower", CinematicSpritePose.LogoSLower, 14),
        new("circle-right-1", CinematicSpritePose.LogoRightCircleTip, 12),
        new("circle-right-2", CinematicSpritePose.LogoRightCircleTipAndBlade, 18),
        new("circle-right-3", CinematicSpritePose.LogoRightCircleWhole, 25),
        new("circle-left-1", CinematicSpritePose.LogoLeftCircleTip, 12),
        new("circle-left-2", CinematicSpritePose.LogoLeftCircleTipAndBlade, 18),
        new("circle-left-3", CinematicSpritePose.LogoLeftCircleWhole, 25),
    ];
}
