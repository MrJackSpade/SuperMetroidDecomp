namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Immutable required tile/color identities from all compiled retail graphics sets.</summary>
public static class EnemyTileSourceDefinitions
{
    /// <summary>Every distinct graphics-set definition, with the bytes of its tile set.</summary>
    public static IReadOnlyList<EnemyTileSourceDefinition> All { get; } = EnemyTileSources.Build();
}
