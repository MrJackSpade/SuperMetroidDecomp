namespace SuperMetroid.Core.Game;

/// <summary>One ordered bank-$B4 graphics-set member and its native VRAM/palette selector.</summary>
/// <param name="DefinitionPointer">Bank-$A0 enemy-header word identifying the graphics/palette source and actor association; each native list record begins with this word.</param>
/// <param name="VramDestination">Packed second record word, not a literal VRAM address: low byte selects the OBJ palette row, and bits 12..13 select staging placement for a header with special tile-size bit 15; ordinary tile data is staged in list order.</param>
public readonly record struct RoomEnemyGraphicsSetHeader(EnemyDefinitionId DefinitionPointer, ushort VramDestination);
