namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed enemy extended frame displays an enemy artwork catalog holds for gameplay (#1463). The enemies'
/// palettes and colour cycles are the import's, which pre-colours the sheets and their shades in them (#1512).
/// </summary>
public sealed record EnemyGameplayArtworkParts
{
    /// <summary>Extended frames' pictures and display bindings.</summary>
    public EnemyExtendedFrameDisplayCatalog? ExtendedFrameDisplays { get; init; } = null;
}
