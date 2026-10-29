using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>One ordered bank-$B4 graphics-set member: the enemy it loads, where its colours go and where its tiles stage.</summary>
/// <param name="DefinitionId">Bank-$A0 enemy-header word identifying the graphics/palette source and actor association; each native list record begins with this word.</param>
/// <param name="ColourStore">The shared sprite colour store the room loads the member's colours into.</param>
/// <param name="StagingBlock">The fixed staging block, 0 through 3, used when the member's definition stages its tiles at a fixed block instead of after the previous member's; ordinary tile data is staged in list order.</param>
public readonly record struct RoomEnemyGraphicsSetHeader(EnemyDefinitionId DefinitionId, SpriteColourStore ColourStore, int StagingBlock = 0)
{
    /// <summary>The largest staging block the record's two placement bits select.</summary>
    public const int MaximumStagingBlock = 3;

    /// <summary>The fixed staging block, 0 through 3.</summary>
    public int StagingBlock { get; init; } = Ensure.BetweenInclusive(0, MaximumStagingBlock, StagingBlock);
}
