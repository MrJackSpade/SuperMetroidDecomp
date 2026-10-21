using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>The enemies, body, wings, body state and wing state of one create pair result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Body">The body.</param>
/// <param name="Wings">The wings.</param>
/// <param name="BodyState">The body state.</param>
/// <param name="WingState">The wing state.</param>
internal readonly record struct CreatePairResult(RoomEnemySystem Enemies, RoomEnemySlot Body, RoomEnemySlot Wings, KiHunterEnemyState BodyState, KiHunterEnemyState WingState);
