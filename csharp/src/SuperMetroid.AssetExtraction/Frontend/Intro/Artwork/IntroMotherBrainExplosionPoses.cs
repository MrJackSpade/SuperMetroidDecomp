using static SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroMotherBrainExplosionSpriteDefinitions;

using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// Selects the pose of each frame of the intro Mother Brain's small and big explosions (<see cref="IntroMotherBrainExplosionSpriteDefinitions"/>).
/// </summary>
internal static class IntroMotherBrainExplosionPoses
{
    /// <summary>The pose of explosion frame <paramref name="frame"/>, 0 through 5, of the small or <paramref name="big"/> effect.</summary>
    internal static CinematicSpritePose FramePose(bool big, int frame) =>
        (uint)frame < 6 ? Frames[(big ? 6 : 0) + frame].Pose : throw new ArgumentOutOfRangeException(nameof(frame));
}
