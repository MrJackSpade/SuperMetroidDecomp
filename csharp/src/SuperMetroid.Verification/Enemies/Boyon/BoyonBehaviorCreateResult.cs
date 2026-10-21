using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Boyon;

/// <summary>The first, second and third of one boyon behavior create result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct BoyonBehaviorCreateResult(RoomEnemySystem First, RoomEnemySlot Second, BoyonEnemyState Third);
