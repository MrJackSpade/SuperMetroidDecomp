using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>The system and body of one run spore spawn program result.</summary>
/// <param name="System">The system.</param>
/// <param name="Body">The body.</param>
internal readonly record struct RunSporeSpawnProgramResult(RoomEnemySystem System, RoomEnemySlot Body);
