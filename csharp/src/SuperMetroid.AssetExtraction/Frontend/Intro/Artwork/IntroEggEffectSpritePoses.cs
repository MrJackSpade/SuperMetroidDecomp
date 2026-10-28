using static SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroEggEffectSpriteDefinitions;

using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// Looks up the egg effect sprite poses (<see cref="IntroEggEffectSpriteDefinitions"/>) by frame.
/// </summary>
internal static class IntroEggEffectSpritePoses
{
    /// <summary>The pose of effect frame <paramref name="frame"/>, 0 through 10.</summary>
    internal static CinematicSpritePose FramePose(int frame) =>
        Frames[Ensure.BetweenInclusive(0, 10, frame)].Pose;
}
