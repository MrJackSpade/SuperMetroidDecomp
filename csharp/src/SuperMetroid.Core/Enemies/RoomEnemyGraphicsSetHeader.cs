namespace SuperMetroid.Core.Enemies;

/// <summary>One ordered graphics-set member: the enemy it loads and where its tiles stage.</summary>
/// <remarks>
/// The colours a member draws in are its installed sheet's (#1512): the import pre-colours each member's sheets in the
/// colours its graphics set loads for it, so a member names no colour store.
/// </remarks>
/// <param name="DefinitionId">enemy-header word identifying the graphics source and actor association; each list record begins with this word.</param>
/// <param name="StagingBlock">The fixed staging block, 0 through 3, used when the member's definition stages its tiles at a fixed block instead of after the previous member's; ordinary tile data is staged in list order.</param>
public readonly record struct RoomEnemyGraphicsSetHeader(EnemyDefinitionId DefinitionId, int StagingBlock = 0)
{
    /// <summary>The largest staging block the record's two placement bits select.</summary>
    public const int MaximumStagingBlock = 3;

    /// <summary>The fixed staging block, 0 through 3.</summary>
    public int StagingBlock { get; init; } = Ensure.BetweenInclusive(0, MaximumStagingBlock, StagingBlock);
}
