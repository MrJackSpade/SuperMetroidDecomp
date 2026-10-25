namespace SuperMetroid.Core.Enemies;

/// <summary>One literal four-byte bank-$B4 room graphics-set record plus resolved data.</summary>
/// <param name="DefinitionId">Record +$00 bank-$A0 enemy header identity used to resolve character and palette artwork.</param>
/// <param name="PaletteRow">The member's OBJ palette row, 0 through 255.</param>
/// <param name="TileIndexBase">Accumulated character index used as the enemy's sprite tile offset, advancing by each member's tile characters.</param>
/// <param name="Definition">Resolved immutable header corresponding to the record's enemy identity.</param>
/// <param name="StagingOffset">Byte offset into the enemy tile area, following the previous member or at the record's fixed block.</param>
/// <param name="TileByteCount">The bytes of the member's tile set.</param>
public readonly record struct RoomEnemyGraphicsSetEntry(
    EnemyDefinitionId DefinitionId,
    int PaletteRow,
    int TileIndexBase,
    RoomEnemyDefinition Definition,
    int StagingOffset,
    int TileByteCount);
