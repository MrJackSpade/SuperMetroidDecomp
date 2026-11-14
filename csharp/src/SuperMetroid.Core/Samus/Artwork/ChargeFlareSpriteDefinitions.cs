using SuperMetroid.Core.Samus.Projectiles;

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

    /// <summary>The place among the four main flare compositions of each main flare picture; both facings show the same.</summary>
    internal static readonly IReadOnlyDictionary<ChargeFlarePicture, int> MainFlareCompositions = new Dictionary<ChargeFlarePicture, int>
    {
        [ChargeFlarePicture.MainFirst] = 0,
        [ChargeFlarePicture.MainSecond] = 1,
        [ChargeFlarePicture.MainThird] = 2,
        [ChargeFlarePicture.MainFourth] = 3,
    };

    /// <summary>The place among its facing's six compositions of each spark ring picture.</summary>
    internal static readonly IReadOnlyDictionary<ChargeFlarePicture, int> SparkCompositions = new Dictionary<ChargeFlarePicture, int>
    {
        [ChargeFlarePicture.SparksFirst] = 0,
        [ChargeFlarePicture.SparksSecond] = 1,
        [ChargeFlarePicture.SparksThird] = 2,
        [ChargeFlarePicture.SparksFourth] = 3,
        [ChargeFlarePicture.SparksFifth] = 4,
        [ChargeFlarePicture.SparksSixth] = 5,
    };
}
