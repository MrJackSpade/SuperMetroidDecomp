namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The charge/Hyper flare and its directional sparks.</summary>
public static class ChargeFlareSpriteDefinitions
{
    /// <summary>Installed JSON resource filename for charge/Hyper/grapple flare and spark OAM compositions, separate from ordinary projectile compositions.</summary>
    public const string FileName = "charge-flare-compositions.json";
    /// <summary>$93:A225/A22B, FlareSpritemapTable_IndexOffsets: both facing rows select the main flare at offset zero.</summary>
    public const ushort MainFlareSelectorOffset = 0;

    /// <summary>How many compositions each flare family holds: four main flares, then the right slow, right fast, left slow and left fast spark groups.</summary>
    private static readonly ProjectileSpriteGroupSize[] Groups =
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
    /// The composition each of the fifty-four native flare-table positions selects: thirty main-flare phases, then six
    /// phases each of right slow, right fast, left slow and left fast sparks. Repeated main-flare phases stay separate
    /// positions.
    /// </summary>
    public static IReadOnlyList<ProjectileSpriteId> Selectors { get; } = [.. Enumerable.Range(0, 54).Select(Selector)];

    /// <summary>The composition flare-table position <paramref name="index"/> selects.</summary>
    private static ProjectileSpriteId Selector(int index) =>
        All[index >= 30 ? index - 26 : index < 8 ? index % 2 : index < 16 ? 2 - index % 2 : 2 + index % 2];
}
