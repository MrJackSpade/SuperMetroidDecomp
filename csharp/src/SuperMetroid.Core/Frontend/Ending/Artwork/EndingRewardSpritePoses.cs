using SuperMetroid.Core.Frontend.Intro;
using static SuperMetroid.Core.Frontend.Ending.Artwork.EndingRewardSpriteDefinitions;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Looks up the reward-scene sprite poses (<see cref="EndingRewardSpriteDefinitions"/>) by frame.
/// </summary>
public static class EndingRewardSpritePoses
{
    /// <summary>The cinematic pose that shows <paramref name="frame"/>.</summary>
    internal static CinematicSpritePose FramePose(EndingRewardSpriteFrame frame) =>
        Ensure.Listed(FramePoses, frame);
}
