using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>The system and enemy of one create literal no op combat system result.</summary>
/// <param name="System">The system.</param>
/// <param name="Enemy">The enemy.</param>
internal readonly record struct CreateLiteralNoOpCombatSystemResult(RoomEnemySystem System, RoomEnemySlot Enemy);
