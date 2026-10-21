using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The system and rinka of one run rinka program result.</summary>
/// <param name="System">The system.</param>
/// <param name="Rinka">The rinka.</param>
internal readonly record struct RunRinkaProgramResult(RoomEnemySystem System, RoomEnemySlot Rinka);
