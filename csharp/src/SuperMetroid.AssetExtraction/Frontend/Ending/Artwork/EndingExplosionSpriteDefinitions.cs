using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The compositions the eight ending explosion actors show.</summary>
public static class EndingExplosionSpriteDefinitions
{
    /// <summary>
    /// The ten four-part planet records (four damage, four flash and two lava frames), then the large glowing planet, the
    /// two 20-part supernova poses, the starfield, the 20-part silhouette and the final 37-part afterglow.
    /// </summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("planet-damage-0", CinematicSpritePose.PlanetDamage0, 4),
        new("planet-damage-1", CinematicSpritePose.PlanetDamage1, 4),
        new("planet-damage-2", CinematicSpritePose.PlanetDamage2, 4),
        new("planet-damage-3", CinematicSpritePose.PlanetDamage3, 4),
        new("planet-flash-0", CinematicSpritePose.PlanetFlash0, 4),
        new("planet-flash-1", CinematicSpritePose.PlanetFlash1, 4),
        new("planet-flash-2", CinematicSpritePose.PlanetFlash2, 4),
        new("planet-flash-3", CinematicSpritePose.PlanetFlash3, 4),
        new("lava-0", CinematicSpritePose.PlanetLava0, 4),
        new("lava-1", CinematicSpritePose.PlanetLava1, 4),
        new("glow-0", CinematicSpritePose.PlanetGlow0, 12),
        new("glow-1", CinematicSpritePose.PlanetGlow1, 20),
        new("glow-2", CinematicSpritePose.PlanetGlow2, 20),
        new("starfield", CinematicSpritePose.EndingStarfield, 53),
        new("silhouette", CinematicSpritePose.PlanetSilhouette, 20),
        new("afterglow", CinematicSpritePose.PlanetAfterglow, 37),
    ];
}
