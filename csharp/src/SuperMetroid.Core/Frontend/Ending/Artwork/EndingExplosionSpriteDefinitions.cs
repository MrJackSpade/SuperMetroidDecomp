using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The compositions the eight ending explosion actors show.</summary>
public static class EndingExplosionSpriteDefinitions
{
    /// <summary>The composite compositions, valued by their position in <see cref="Frames"/> after the ten planet records.</summary>
    internal enum Pose
    {
        /// <summary>Large glowing planet frame immediately after the ten small planet records.</summary>
        Glow = PlanetRecordCount,
        /// <summary>First 20-part supernova pose.</summary>
        SupernovaFirst = 11,
        /// <summary>Second 20-part supernova pose.</summary>
        SupernovaSecond = 12,
        /// <summary>The starfield.</summary>
        Stars = 13,
        /// <summary>20-part silhouette pose following the supernova records.</summary>
        Silhouette = 14,
        /// <summary>Final 37-part afterglow pose.</summary>
        Afterglow = 15,
    }

    /// <summary>Four damage, four flash and two lava planet records precede the composite poses.</summary>
    private const int PlanetRecordCount = 10;

    /// <summary>The ten four-part planet records, then the composite poses in <see cref="Pose"/> order.</summary>
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

    /// <summary>The cinematic pose of planet record <paramref name="record"/>: four damage, four flash and two lava poses.</summary>
    internal static CinematicSpritePose PlanetPose(int record) =>
        (uint)record < PlanetRecordCount ? Frames[record].Pose : throw new ArgumentOutOfRangeException(nameof(record));

    /// <summary>The cinematic pose of the composite <paramref name="pose"/>.</summary>
    internal static CinematicSpritePose PoseOf(Pose pose) =>
        pose is >= Pose.Glow and <= Pose.Afterglow ? Frames[(int)pose].Pose : throw new ArgumentOutOfRangeException(nameof(pose));
}
