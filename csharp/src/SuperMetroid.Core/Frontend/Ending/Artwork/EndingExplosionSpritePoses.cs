using SuperMetroid.Core.Frontend.Intro;
using static SuperMetroid.Core.Frontend.Ending.Artwork.EndingExplosionSpriteDefinitions;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Looks up the planet-explosion sprite poses (<see cref="EndingExplosionSpriteDefinitions"/>) by record and pose.
/// </summary>
public static class EndingExplosionSpritePoses
{
    /// <summary>The cinematic pose of planet record <paramref name="record"/>: four damage, four flash and two lava poses.</summary>
    internal static CinematicSpritePose PlanetPose(int record) =>
        Ensure.Element(PlanetRecords, record).Pose;

    /// <summary>The cinematic pose of the composite <paramref name="pose"/>.</summary>
    internal static CinematicSpritePose PoseOf(Pose pose) =>
        Ensure.Listed(Composites, pose).Pose;
}
