namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The burst a hit of Samus's weapon shows at an enemy it hurts (#1713): one of her weapon effects, pictured on her
/// projectile sheets with her beams' other pictures and drawn in her beams' colouring. Its four compositions show its four
/// mirrored pieces as they spread from its centre (#1762).
/// </summary>
public static class HitBurstSpriteDefinitions
{
    /// <summary>The burst's pieces gathered tight about its centre, as it first shows.</summary>
    public static ProjectileSpriteId Tight { get; } = new(ProjectileSpriteGroup.HitBurst, nameof(Tight));

    /// <summary>The burst's pieces opening out from its centre.</summary>
    public static ProjectileSpriteId Opening { get; } = new(ProjectileSpriteGroup.HitBurst, nameof(Opening));

    /// <summary>The burst's pieces spread wide.</summary>
    public static ProjectileSpriteId Wide { get; } = new(ProjectileSpriteGroup.HitBurst, nameof(Wide));

    /// <summary>The burst's pieces scattered furthest, as it last shows.</summary>
    public static ProjectileSpriteId Scattered { get; } = new(ProjectileSpriteGroup.HitBurst, nameof(Scattered));

    /// <summary>The burst's four compositions, nearest its centre first.</summary>
    public static IReadOnlyList<ProjectileSpriteId> All { get; } = [Tight, Opening, Wide, Scattered];
}
