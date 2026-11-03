using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>A loaded enemy population and the OBJ characters its queued tile uploads produce.</summary>
/// <param name="Enemies">The loaded enemy system.</param>
/// <param name="SpriteCharacters">The OBJ character table after the room's enemy tile uploads land.</param>
internal readonly record struct LoadResult(RoomEnemySystem Enemies, byte[] SpriteCharacters);
