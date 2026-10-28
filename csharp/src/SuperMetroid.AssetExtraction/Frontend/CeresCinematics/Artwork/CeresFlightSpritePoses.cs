using SuperMetroid.Core.Frontend.Intro;
using static SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork.CeresFlightSpriteDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Selects the Ceres flight's vortex pose (<see cref="CeresFlightSpriteDefinitions"/>) for each frame of its animation.
/// </summary>
internal static class CeresFlightSpritePoses
{
    /// <summary>The cinematic pose of a purple vortex drawing.</summary>
    internal static CinematicSpritePose VortexPose(CeresVortexFrame frame) => frame switch
    {
        CeresVortexFrame.Even => Frames[4].Pose,
        CeresVortexFrame.Odd => Frames[5].Pose,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
