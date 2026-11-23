using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The compositions the eight ending explosion actors show.</summary>
public static partial class EndingExplosionSpriteDefinitions
{
    /// <summary>
    /// The ten four-part planet records (four damage, four flash and two lava frames), then the large glowing planet, the
    /// two 20-part supernova poses, the starfield, the 20-part silhouette and the final 37-part afterglow.
    /// </summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("planet-damage-0", CinematicSpritePose.PlanetRumbleDeepShadow, 4),
        new("planet-damage-1", CinematicSpritePose.PlanetRumbleShadow, 4),
        new("planet-damage-2", CinematicSpritePose.PlanetRumbleLit, 4),
        new("planet-damage-3", CinematicSpritePose.PlanetRumbleBright, 4),
        new("planet-flash-0", CinematicSpritePose.PlanetCracking, 4),
        new("planet-flash-1", CinematicSpritePose.PlanetFracturing, 4),
        new("planet-flash-2", CinematicSpritePose.PlanetBreakingApart, 4),
        new("planet-flash-3", CinematicSpritePose.PlanetCrumbling, 4),
        new("lava-0", CinematicSpritePose.PlanetLavaDark, 4),
        new("lava-1", CinematicSpritePose.PlanetLavaBright, 4),
        new("glow-0", CinematicSpritePose.PlanetGlowTightHalo, 12),
        new("glow-1", CinematicSpritePose.PlanetGlowSpreadingHalo, 20),
        new("glow-2", CinematicSpritePose.PlanetGlowWideHalo, 20),
        new("starfield", CinematicSpritePose.EndingStarfield, 53),
        new("silhouette", CinematicSpritePose.PlanetSilhouette, 20),
        new("afterglow", CinematicSpritePose.PlanetAfterglow, 37),
    ];
}
