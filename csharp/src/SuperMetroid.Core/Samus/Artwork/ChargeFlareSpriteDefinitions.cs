namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The charge/Hyper flare and its directional sparks.</summary>
public static class ChargeFlareSpriteDefinitions
{
    /// <summary>How many compositions each flare family holds: four main flares, then the right slow, right fast, left slow and left fast spark groups.</summary>
    internal static readonly ProjectileSpriteGroupSize[] Groups =
    [
        new(ProjectileSpriteGroup.FlareMain, 4),
        new(ProjectileSpriteGroup.FlareSlowSparksRight, 6),
        new(ProjectileSpriteGroup.FlareFastSparksRight, 6),
        new(ProjectileSpriteGroup.FlareSlowSparksLeft, 6),
        new(ProjectileSpriteGroup.FlareFastSparksLeft, 6),
    ];

    /// <summary>The twenty-eight distinct flare and spark compositions, in family order.</summary>
    public static IReadOnlyList<ProjectileSpriteId> All { get; } = [.. Groups.SelectMany(group => group.Members)];

    /// <summary>
    /// The main flare composition each of the main flare's thirty pictures shows, by its place among the four main
    /// compositions: the first two alternate for eight pictures, the third and second for eight more, and the third and
    /// fourth for the last fourteen, which the main flare's loop repeats. Both facings show the same pictures.
    /// </summary>
    internal static readonly int[] MainFlarePictures =
    [
        0, 1, 0, 1, 0, 1, 0, 1,
        2, 1, 2, 1, 2, 1, 2, 1,
        2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3, 2, 3,
    ];
}
