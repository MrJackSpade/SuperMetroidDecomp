namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy's tile and palette indexes in the room's graphics set.</summary>
/// <param name="TileIndex">The tile index.</param>
/// <param name="PaletteIndex">The palette index.</param>
internal readonly record struct EnemyGraphicsIndexes(int TileIndex, int PaletteIndex);
