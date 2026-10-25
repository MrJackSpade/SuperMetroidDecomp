using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The compositions the eight ending explosion actors show.</summary>
public static class EndingExplosionSpriteDefinitions
{
    /// <summary>The composite compositions, in the order <see cref="Frames"/> lists them after the ten planet records.</summary>
    internal enum Pose
    {
        /// <summary>Large glowing planet frame immediately after the ten small planet records.</summary>
        Glow,
        /// <summary>First 20-part supernova pose.</summary>
        SupernovaFirst,
        /// <summary>Second 20-part supernova pose.</summary>
        SupernovaSecond,
        /// <summary>The starfield.</summary>
        Stars,
        /// <summary>20-part silhouette pose following the supernova records.</summary>
        Silhouette,
        /// <summary>Final 37-part afterglow pose.</summary>
        Afterglow,
    }

    /// <summary>The ten four-part planet records: four damage, four flash and two lava frames.</summary>
    private static readonly CinematicSpriteFrameDefinition[] PlanetRecords =
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
    ];

    /// <summary>The composition each composite pose draws.</summary>
    private static readonly Dictionary<Pose, CinematicSpriteFrameDefinition> Composites = new()
    {
        [Pose.Glow] = new("glow-0", new(91), 12),
        [Pose.SupernovaFirst] = new("glow-1", new(92), 20),
        [Pose.SupernovaSecond] = new("glow-2", new(93), 20),
        [Pose.Stars] = new("starfield", new(94), 53),
        [Pose.Silhouette] = new("silhouette", new(97), 20),
        [Pose.Afterglow] = new("afterglow", new(98), 37),
    };

    /// <summary>The ten four-part planet records, then the composite poses in <see cref="Pose"/> order.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
        [.. PlanetRecords, .. Enum.GetValues<Pose>().Select(pose => Composites[pose])];

    /// <summary>The cinematic pose of planet record <paramref name="record"/>: four damage, four flash and two lava poses.</summary>
    internal static CinematicSpritePose PlanetPose(int record) =>
        (uint)record < PlanetRecords.Length ? PlanetRecords[record].Pose : throw new ArgumentOutOfRangeException(nameof(record));

    /// <summary>The cinematic pose of the composite <paramref name="pose"/>.</summary>
    internal static CinematicSpritePose PoseOf(Pose pose) =>
        Composites.TryGetValue(pose, out CinematicSpriteFrameDefinition frame) ? frame.Pose : throw new ArgumentOutOfRangeException(nameof(pose));
}
