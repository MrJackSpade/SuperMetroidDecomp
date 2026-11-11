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
        new("planet-damage-0", new(83), 4),
        new("planet-damage-1", new(84), 4),
        new("planet-damage-2", new(85), 4),
        new("planet-damage-3", new(86), 4),
        new("planet-flash-0", new(87), 4),
        new("planet-flash-1", new(88), 4),
        new("planet-flash-2", new(89), 4),
        new("planet-flash-3", new(90), 4),
        new("lava-0", new(95), 4),
        new("lava-1", new(96), 4),
        new("glow-0", new(91), 12),
        new("glow-1", new(92), 20),
        new("glow-2", new(93), 20),
        new("starfield", new(94), 53),
        new("silhouette", new(97), 20),
        new("afterglow", new(98), 37),
    ];
}
