using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Sparks;

/// <summary>The first, second and third of one spark movement definitions start result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct SparkMovementDefinitionsStartResult(RoomEnemySystem First, RoomEnemySlot Second, SparkEnemyState Third);
