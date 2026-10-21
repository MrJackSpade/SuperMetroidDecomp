using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The enemies and torizo of one create bomb torizo attack fixture result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Torizo">The torizo.</param>
internal readonly record struct CreateBombTorizoAttackFixtureResult(RoomEnemySystem Enemies, RoomEnemySlot Torizo);
