namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The tile set an enemy header names, as the import lays graphics sets out by it (#1513).</summary>
/// <param name="ByteCount">The size of the enemy's tile set in bytes.</param>
internal readonly record struct EnemyHeaderTileSet(int ByteCount);
