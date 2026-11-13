namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The burst a hit of Samus's weapon shows at an enemy it hurts (#1713): one of her weapon effects, pictured on her
/// projectile sheets with her beams' other pictures and drawn in her beams' colouring.
/// </summary>
public static class HitBurstSpriteDefinitions
{
    /// <summary>How many compositions the burst holds: four poses, shown in order as its pieces spread.</summary>
    private static readonly ProjectileSpriteGroupSize[] Groups =
    [
        new(ProjectileSpriteGroup.HitBurst, 4),
    ];

    /// <summary>The burst's four compositions, nearest its centre first.</summary>
    public static IReadOnlyList<ProjectileSpriteId> All { get; } = [.. Groups.SelectMany(group => group.Members)];
}
