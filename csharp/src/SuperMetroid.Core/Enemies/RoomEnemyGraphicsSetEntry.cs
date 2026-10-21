namespace SuperMetroid.Core.Enemies;

/// <summary>One literal four-byte bank-$B4 room graphics-set record plus resolved data.</summary>
/// <param name="DefinitionId">Record +$00 bank-$A0 enemy header identity used to resolve character and palette artwork.</param>
/// <param name="PaletteRow">The member's OBJ palette row, 0 through 255.</param>
/// <param name="VramTilesIndex">Accumulated character index used as the enemy's sprite tile offset, advancing by each raw header size divided by 32, 0 through 65535.</param>
/// <param name="Definition">Resolved immutable header corresponding to the record's enemy identity.</param>
/// <param name="StagingOffset">Byte offset from the enemy VRAM byte base, selected sequentially or from the explicit placement bits.</param>
/// <param name="TileByteCount">Actual planar character upload size in bytes, with the header size word's placement bit removed.</param>
public readonly record struct RoomEnemyGraphicsSetEntry(
    EnemyDefinitionId DefinitionId,
    int PaletteRow,
    int VramTilesIndex,
    RoomEnemyDefinition Definition,
    int StagingOffset,
    int TileByteCount);
