using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CeresFlightSpriteDefinitions"/> that only verification reads.</summary>
internal static class CeresFlightSpriteDefinitionsConstants
{
    /// <summary>The large asteroids of the approach, at OBJ priority zero.</summary>
    internal static CinematicSpritePose LargeAsteroids => CeresFlightSpriteDefinitions.Frames[1].Pose;
}
