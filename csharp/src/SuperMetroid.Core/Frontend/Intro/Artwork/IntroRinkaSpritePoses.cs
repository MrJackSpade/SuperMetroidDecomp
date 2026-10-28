using static SuperMetroid.Core.Frontend.Intro.Artwork.IntroRinkaSpriteDefinitions;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Looks up the intro Rinka sprite poses (<see cref="IntroRinkaSpriteDefinitions"/>) by frame.
/// </summary>
internal static class IntroRinkaSpritePoses
{
    /// <summary>The pose of Rinka frame <paramref name="frame"/>, 0 through 2.</summary>
    internal static CinematicSpritePose FramePose(int frame) =>
        Frames[Ensure.BetweenInclusive(0, 2, frame)].Pose;
}
