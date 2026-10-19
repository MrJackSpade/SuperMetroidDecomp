namespace SuperMetroid.Core.Enemies;

/// <summary>One ordered bank-$B4 graphics-set member and its native VRAM/palette selector.</summary>
/// <param name="DefinitionPointer">Bank-$A0 enemy-header word identifying the graphics/palette source and actor association; each native list record begins with this word.</param>
/// <param name="PaletteRow">The OBJ palette row the member's colours load into, 0 through 255.</param>
/// <param name="StagingBlock">The fixed staging block, 0 through 3, used when the member's definition stages its tiles at a fixed block instead of after the previous member's; ordinary tile data is staged in list order.</param>
public readonly record struct RoomEnemyGraphicsSetHeader(EnemyDefinitionId DefinitionPointer, int PaletteRow, int StagingBlock = 0)
{
    /// <summary>The largest staging block the record's two placement bits select.</summary>
    public const int MaximumStagingBlock = 3;

    /// <summary>The OBJ palette row, 0 through 255.</summary>
    public int PaletteRow { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, PaletteRow);

    /// <summary>The fixed staging block, 0 through 3.</summary>
    public int StagingBlock { get; init; } = Ensure.BetweenInclusive(0, MaximumStagingBlock, StagingBlock);
}
