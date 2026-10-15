namespace SuperMetroid.Core.Enemies;

/// <summary>One literal four-byte bank-$B4 room graphics-set record plus resolved data.</summary>
/// <param name="DefinitionPointer">Record +$00 bank-$A0 enemy header identity used to resolve character and palette artwork.</param>
/// <param name="VramDestination">Record +$02 packed palette/placement word, not a direct VRAM address: its low byte selects the OBJ palette and high bits participate in explicit tile placement.</param>
/// <param name="VramTilesIndex">Accumulated character index used as the enemy's sprite tile offset, advancing by each raw header size divided by 32.</param>
/// <param name="Definition">Resolved immutable header corresponding to the record's enemy identity.</param>
/// <param name="StagingOffset">Byte offset from the enemy VRAM byte base, selected sequentially or from the explicit placement bits.</param>
/// <param name="TileByteCount">Actual planar character upload size in bytes, with the header size word's placement bit removed.</param>
public readonly record struct RoomEnemyGraphicsSetEntry(
    EnemyDefinitionId DefinitionPointer,
    ushort VramDestination,
    ushort VramTilesIndex,
    RoomEnemyDefinition Definition,
    int StagingOffset,
    int TileByteCount);
