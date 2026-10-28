using static SuperMetroid.Core.Frontend.Intro.Artwork.IntroScientistSpriteDefinitions;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Looks up the intro scientist sprite poses (<see cref="IntroScientistSpriteDefinitions"/>) by frame.
/// </summary>
internal static class IntroScientistSpritePoses
{
    /// <summary>The pose of scientist frame <paramref name="index"/>, 0 through 9.</summary>
    internal static CinematicSpritePose FramePose(int index) =>
        Frames[Ensure.BetweenInclusive(0, 9, index)].Pose;
}
