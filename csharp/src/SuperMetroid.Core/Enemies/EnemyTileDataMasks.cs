namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The sizes a room graphics set lays its members' tiles out by: tile index bases advance a member's tile count, and the
/// enemy tile area bounds where its members' tiles go.
/// </summary>
internal static class EnemyTileDataMasks
{
    /// <summary>The bytes one fixed staging block spans.</summary>
    internal const int StagingBlockByteCount = 0x200;

    /// <summary>The bytes one enemy tile holds; a member's tile index base advances one per tile.</summary>
    internal const int TileByteCount = 32;

    /// <summary>The bytes of the enemy tile area: 320 tiles, through the end of the sprites' tile memory.</summary>
    internal const int EnemyTileAreaByteCount = 320 * TileByteCount;

    /// <summary>Where the set's first member that follows the previous one stages its tiles: 64 tiles into the enemy tile area.</summary>
    internal const int OrdinaryTilesStart = 64 * TileByteCount;
}
