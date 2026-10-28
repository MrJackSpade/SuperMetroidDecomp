using static SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroMotherBrainSpriteDefinitions;

using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// Looks up the intro Mother Brain sprite poses (<see cref="IntroMotherBrainSpriteDefinitions"/>) by frame.
/// </summary>
internal static class IntroMotherBrainSpritePoses
{
    /// <summary>The pose of Mother Brain frame <paramref name="frame"/>, 0 through 2.</summary>
    internal static CinematicSpritePose FramePose(int frame) =>
        Frames[Ensure.BetweenInclusive(0, 2, frame)].Pose;
}
