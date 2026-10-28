using static SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroDiscoveryActorSpriteDefinitions;

using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// Looks up the discovery scene's egg and baby sprite poses (<see cref="IntroDiscoveryActorSpriteDefinitions"/>) by state.
/// </summary>
internal static class IntroDiscoveryActorSpritePoses
{
    /// <summary>The pose of egg state <paramref name="frame"/>, 0 (intact) through 15 (last remnant).</summary>
    internal static CinematicSpritePose EggFramePose(int frame) =>
        Frames[Ensure.BetweenInclusive(0, 15, frame)].Pose;

    /// <summary>The pose of confused-baby state <paramref name="frame"/>, 0 through 2.</summary>
    internal static CinematicSpritePose BabyFramePose(int frame) =>
        Frames[16 + Ensure.BetweenInclusive(0, 2, frame)].Pose;
}
